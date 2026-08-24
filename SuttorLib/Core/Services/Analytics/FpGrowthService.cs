using SuttorLibrary.DTOs;

namespace SuttorLib.Core.Services.Analytics
{
    /// <summary>
    /// FP-Growth association rule mining over user reading transactions.
    /// Builds an FP-tree and mines frequent itemsets without candidate generation,
    /// then derives rules with support / confidence / lift.
    /// </summary>
    public static class FpGrowthService
    {
        private sealed class FpNode(string item, FpNode? parent)
        {
            public string Item { get; } = item;
            public FpNode? Parent { get; } = parent;
            public Dictionary<string, FpNode> Children { get; } = new();
            public FpNode? NodeLink { get; set; }
            public int Count { get; set; }
        }

        /// <param name="transactions">Each transaction is the set of item ids a user interacted with.</param>
        public static List<AssociationRuleDto> Mine(
            List<HashSet<string>> transactions,
            Dictionary<string, string> itemLabels,
            double minSupport = 0.02,
            double minConfidence = 0.3,
            double minLift = 1.0,
            int maxRules = 50)
        {
            int n = transactions.Count;
            if (n == 0) return [];

            int minCount = Math.Max(1, (int)Math.Ceiling(minSupport * n));

            // ---- Pass 1: item frequencies ----
            var freq = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var t in transactions)
                foreach (var item in t)
                    freq[item] = freq.GetValueOrDefault(item) + 1;

            bool IsFrequent(string item) => freq.GetValueOrDefault(item) >= minCount;

            // ---- Pass 2: build the FP-tree ----
            var root = new FpNode("", null);
            var header = new Dictionary<string, FpNode>(StringComparer.Ordinal);

            foreach (var t in transactions)
            {
                var ordered = t.Where(IsFrequent)
                    .OrderByDescending(i => freq[i])
                    .ThenBy(i => i, StringComparer.Ordinal);

                var node = root;
                foreach (var item in ordered)
                {
                    if (!node.Children.TryGetValue(item, out var child))
                    {
                        child = new FpNode(item, node);
                        node.Children[item] = child;

                        if (!header.TryGetValue(item, out var head))
                            header[item] = child;
                        else
                        {
                            while (head.NodeLink != null) head = head.NodeLink!;
                            head.NodeLink = child;
                        }
                    }
                    child.Count++;
                    node = child;
                }
            }

            // ---- Mining ----
            // Frequent itemset (sorted, comma-joined) -> absolute support count.
            var mined = new Dictionary<string, int>(StringComparer.Ordinal);
            void Record(List<string> itemset, int count)
            {
                var key = string.Join(",", itemset.OrderBy(x => x, StringComparer.Ordinal));
                mined[key] = Math.Max(mined.GetValueOrDefault(key), count);
            }

            void Mine(Dictionary<string, FpNode> hTable, List<string> prefix)
            {
                // Grow from the least frequent item upward so longer patterns
                // are found through conditional bases.
                foreach (var (item, headNode) in hTable.OrderBy(kv => kv.Value.Count).ThenBy(kv => kv.Key, StringComparer.Ordinal))
                {
                    int itemCount = 0;
                    var patternBase = new List<(List<string> Path, int Count)>();

                    for (var node = headNode; node != null; node = node.NodeLink!)
                    {
                        itemCount += node.Count;

                        var path = new List<string>();
                        for (var p = node.Parent!; p.Parent != null; p = p.Parent!)
                            path.Add(p.Item);
                        if (path.Count > 0)
                            patternBase.Add((path, node.Count));
                    }

                    var itemset = new List<string>(prefix) { item };
                    Record(itemset, itemCount);

                    // Conditional frequency of items above this one
                    var condFreq = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var (path, count) in patternBase)
                        foreach (var it in path)
                            condFreq[it] = condFreq.GetValueOrDefault(it) + count;

                    var keptItems = condFreq.Where(kv => kv.Value >= minCount)
                        .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
                    if (keptItems.Count == 0) continue;

                    // Build the conditional FP-tree
                    var condRoot = new FpNode("", null);
                    var condHeader = new Dictionary<string, FpNode>(StringComparer.Ordinal);
                    foreach (var (path, count) in patternBase)
                    {
                        var filtered = path.Where(keptItems.ContainsKey)
                            .OrderByDescending(i => keptItems[i])
                            .ThenBy(i => i, StringComparer.Ordinal);
                        InsertPath(condRoot, condHeader, filtered, count);
                    }

                    if (condHeader.Count > 0)
                        Mine(condHeader, itemset);
                }
            }

            static void InsertPath(FpNode rootNode, Dictionary<string, FpNode> hTable,
                IEnumerable<string> items, int count)
            {
                var node = rootNode;
                foreach (var it in items)
                {
                    if (!node.Children.TryGetValue(it, out var child))
                    {
                        child = new FpNode(it, node);
                        node.Children[it] = child;

                        if (!hTable.TryGetValue(it, out var head))
                            hTable[it] = child;
                        else
                        {
                            while (head.NodeLink != null) head = head.NodeLink!;
                            head.NodeLink = child;
                        }
                    }
                    child.Count += count;
                    node = child;
                }
            }

            Mine(header, []);

            // ---- Rule generation ----
            double SupportPct(int count) => (double)count / n;
            double ItemSupport(string item) => SupportPct(freq[item]);
            double SetSupport(string key) => SupportPct(mined.GetValueOrDefault(key));

            var rules = new List<(string Antecedent, string Consequent, double Support, double Confidence, double Lift)>();

            foreach (var (key, _) in mined)
            {
                var items = key.Split(',');
                if (items.Length != 2) continue; // keep readable X -> Y rules

                var (a, b) = (items[0], items[1]);

                double confAB = SetSupport(key) / ItemSupport(a);
                double liftAB = confAB / ItemSupport(b);
                if (confAB >= minConfidence && liftAB >= minLift)
                    rules.Add((a, b, SetSupport(key), confAB, liftAB));

                double confBA = SetSupport(key) / ItemSupport(b);
                double liftBA = confBA / ItemSupport(a);
                if (confBA >= minConfidence && liftBA >= minLift)
                    rules.Add((b, a, SetSupport(key), confBA, liftBA));
            }

            return rules
                .OrderByDescending(r => r.Lift)
                .Take(maxRules)
                .Select(r => new AssociationRuleDto(
                    AntecedentTitle: itemLabels.GetValueOrDefault(r.Antecedent, r.Antecedent),
                    ConsequentTitle: itemLabels.GetValueOrDefault(r.Consequent, r.Consequent),
                    Support: Math.Round(r.Support * 100, 2),
                    Confidence: Math.Round(r.Confidence * 100, 2),
                    Lift: Math.Round(r.Lift, 3)
                ))
                .ToList();
        }
    }
}
