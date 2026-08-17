using System.ComponentModel.DataAnnotations;

namespace SuttorLib.Models.Recommender
{
    /// <summary>
    /// Raw shape of a row coming back out of model.Transform(). Types must
    /// match the underlying IDataView columns exactly (all Single/float) —
    /// ML.NET's CreateEnumerable does not coerce int &lt;-&gt; float for you.
    /// </summary>
    public class ScoredCandidate
    {
        [Key]
        public string Id { get; set; }

        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;
        [MaxLength(255)]
        public string BookId { get; set; } = string.Empty;
        public float Score { get; set; } = 0f;
    }

    /// <summary>
    /// Clean, int-keyed result for a scored (user, book) candidate — what the
    /// rest of the app should actually work with.
    /// </summary>
    public record ScoredBook(string UserId, string BookId, float Score);
}
