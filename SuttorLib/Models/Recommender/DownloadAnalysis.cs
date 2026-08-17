using System.ComponentModel.DataAnnotations;

namespace SuttorLib.Models.Recommender
{
    /// <summary>
    /// One observed user/book interaction (currently: a Download row) used both
    /// as training input and, reused with Label = 0, as a "please score this
    /// pair" query at prediction time.
    ///
    /// UserId/BookId are plain floats on purpose: ML.NET turns them into
    /// KeyType columns dynamically via MapValueToKey when the pipeline is
    /// fitted, so we don't need to know the total user/book count up front
    /// (that matters here since the catalog keeps growing).
    /// </summary>
    public class DownloadAnalysis
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;
        [MaxLength(255)]
        public string BookId { get; set; } = string.Empty;

        // Every training row IS a positive interaction (a download happened),
        // so Label is always 1. There are no negative examples to provide —
        // that's exactly what one-class matrix factorization is designed for.
        public float Label { get; set; }
    }
}
