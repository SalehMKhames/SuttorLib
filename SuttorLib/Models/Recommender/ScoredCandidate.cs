namespace SuttorLib.Models.Recommender
{
    /// <summary>
    /// Raw shape of a row coming back out of model.Transform(). Types must
    /// match the underlying IDataView columns exactly (all Single/float) —
    /// ML.NET's CreateEnumerable does not coerce int &lt;-&gt; float for you.
    /// </summary>
    public class ScoredCandidate
    {
        public string UserId { get; set; }
        public string BookId { get; set; }
        public float Score { get; set; }
    }

    /// <summary>
    /// Clean, int-keyed result for a scored (user, book) candidate — what the
    /// rest of the app should actually work with.
    /// </summary>
    public record ScoredBook(string UserId, string BookId, float Score);
}
