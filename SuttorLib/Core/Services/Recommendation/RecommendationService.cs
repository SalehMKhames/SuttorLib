using Microsoft.ML;
using Microsoft.ML.Trainers;
using SuttorLib.Models.Library;
using System.Collections.Generic;
using System.Linq;

namespace SuttorLib.Core.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;

        public RecommendationService()
        {
            _mlContext = new MLContext();
        }

        public void TrainModel(IEnumerable<BookRatingData> trainingData)
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Matrix Factorization requires the data to be typed correctly (Key Types for User and Item IDs)
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: nameof(BookRatingData.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "BookIdEncoded", inputColumnName: nameof(BookRatingData.BookId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                    new MatrixFactorizationTrainer.Options
                    {
                        MatrixColumnIndexColumnName = "UserIdEncoded",
                        MatrixRowIndexColumnName = "BookIdEncoded",
                        LabelColumnName = nameof(BookRatingData.Label),
                        NumberOfIterations = 20,
                        ApproximationRank = 100
                    }));

            _model = pipeline.Fit(dataView);
        }

        public float PredictRating(string userId, int bookId)
        {
            if (_model == null) return 0f;

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<BookRatingData, BookRatingPrediction>(_model);

            var prediction = predictionEngine.Predict(new BookRatingData
            {
                UserId = userId,
                BookId = bookId
            });

            return prediction.Score;
        }

        public IEnumerable<int> GetTopRecommendations(string userId, IEnumerable<int> allBookIds, int topCount = 5)
        {
            if (_model == null) return Enumerable.Empty<int>();

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<BookRatingData, BookRatingPrediction>(_model);

            var predictions = new List<(int BookId, float Score)>();

            foreach (var bookId in allBookIds)
            {
                var prediction = predictionEngine.Predict(new BookRatingData
                {
                    UserId = userId,
                    BookId = bookId
                });
                predictions.Add((bookId, prediction.Score));
            }

            return predictions
                .OrderByDescending(p => p.Score)
                .Take(topCount)
                .Select(p => p.BookId);
        }
    }
}
