using Microsoft.ML.Data;

namespace SuttorLib.Models.Library
{
    public class BookRatingData
    {
        [LoadColumn(0)]
        public string UserId { get; set; } = string.Empty;

        [LoadColumn(1)]
        public int BookId { get; set; }

        [LoadColumn(2)]
        public float Label { get; set; } // Rating
    }

    public class BookRatingPrediction
    {
        public float Label { get; set; }
        public float Score { get; set; }
    }
}
