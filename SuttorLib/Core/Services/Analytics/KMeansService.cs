using SuttorLibrary.DTOs;

namespace SuttorLib.Core.Services.Analytics
{
    /// <summary>
    /// Simple, dependency-free K-Means implementation (Lloyd's algorithm)
    /// used for behavioral user segmentation.
    /// </summary>
    public static class KMeansService
    {
        public static int[] Fit(double[][] data, int k, int maxIterations = 100, int seed = 42)
        {
            if (data.Length == 0) return [];
            k = Math.Clamp(k, 1, data.Length);

            var rng = new Random(seed);
            var n = data.Length;
            var dim = data[0].Length;

            // k-means++ style init: pick first random, then farthest points
            var centroids = new double[k][];
            centroids[0] = (double[])data[rng.Next(n)].Clone();
            for (int c = 1; c < k; c++)
            {
                double bestDist = -1;
                int bestIdx = 0;
                for (int i = 0; i < n; i++)
                {
                    double minDist = double.MaxValue;
                    for (int j = 0; j < c; j++)
                        minDist = Math.Min(minDist, SquaredDistance(data[i], centroids[j]));
                    if (minDist > bestDist)
                    {
                        bestDist = minDist;
                        bestIdx = i;
                    }
                }
                centroids[c] = (double[])data[bestIdx].Clone();
            }

            var assignments = new int[n];

            for (int iter = 0; iter < maxIterations; iter++)
            {
                bool changed = false;

                // Assignment step
                for (int i = 0; i < n; i++)
                {
                    int best = 0;
                    double bestDist = double.MaxValue;
                    for (int c = 0; c < k; c++)
                    {
                        double d = SquaredDistance(data[i], centroids[c]);
                        if (d < bestDist) { bestDist = d; best = c; }
                    }
                    if (assignments[i] != best) changed = true;
                    assignments[i] = best;
                }

                if (!changed && iter > 0) break;

                // Update step
                var sums = new double[k][];
                var counts = new int[k];
                for (int c = 0; c < k; c++) sums[c] = new double[dim];

                for (int i = 0; i < n; i++)
                {
                    counts[assignments[i]]++;
                    for (int d = 0; d < dim; d++)
                        sums[assignments[i]][d] += data[i][d];
                }

                for (int c = 0; c < k; c++)
                {
                    if (counts[c] == 0) continue;
                    for (int d = 0; d < dim; d++)
                        centroids[c][d] = sums[c][d] / counts[c];
                }
            }

            return assignments;
        }

        public static double SquaredDistance(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                var diff = a[i] - b[i];
                sum += diff * diff;
            }
            return sum;
        }
    }
}
