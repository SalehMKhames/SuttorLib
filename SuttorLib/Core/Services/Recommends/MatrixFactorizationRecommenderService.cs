using Microsoft.ML;
using Microsoft.ML.Trainers;
using SuttorLib.Models.Recommender;
using SuttorLibrary.Core.Interfaces;

/// <summary>
/// Trains and scores an implicit-feedback (one-class) matrix factorization
/// model over Download interactions. "One-class" is what makes this work
/// with no pre-existing ratings: every row we feed in is a positive
/// signal (a download happened) and the algorithm itself accounts for
/// everything else being "unobserved" rather than needing explicit
/// negative examples.
/// </summary>
public class MatrixFactorizationRecommenderService
{
    private readonly MLContext _mlContext;

    // Below this many downloads, an individual user's learned vector is
    // too thin to trust — the caller should use the interest-based
    // fallback for them instead. Tune as real usage data comes in.
    public const int MinDownloadsForMf = 3;

    // Don't train at all until the whole system has a minimum amount of
    // signal — an MF model fit on a handful of rows is noise, not a model.
    public const int MinTotalInteractionsToTrain = 50;

    public MatrixFactorizationRecommenderService(MLContext mlContext)
    {
        _mlContext = mlContext;
    }

    /// <summary>
    /// Fits the model, or returns null if there isn't enough system-wide
    /// interaction data yet (expected while the app is new).
    /// </summary>
    public ITransformer? Train(List<InteractionRow> interactions)
    {
        if (interactions.Count < MinTotalInteractionsToTrain)
            return null;

        var trainingRows = interactions
            .Select(i => new DownloadAnalysis { UserId = i.UserId, BookId = i.BookId, Label = 1f })
            .ToList();

        var trainingData = _mlContext.Data.LoadFromEnumerable(trainingRows);

        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: nameof(DownloadAnalysis.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "BookIdEncoded", inputColumnName: nameof(DownloadAnalysis.BookId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "BookIdEncoded",
                LabelColumnName = nameof(DownloadAnalysis.Label),
                LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
                // Standard starting point for implicit-feedback problems;
                // worth tuning once you have real usage data to validate against.
                Alpha = 0.01,
                Lambda = 0.025,
                C = 0.00001,
                NumberOfIterations = 20,
                ApproximationRank = 32
            }));

        return pipeline.Fit(trainingData);
    }

    /// <summary>
    /// Scores a batch of (user, candidate book) pairs in one call — far
    /// cheaper than a PredictionEngine call per pair when generating
    /// recommendations for many users.
    ///
    /// Only pass candidate books that appeared at least once in the
    /// training interactions: a book with zero downloads system-wide was
    /// never in the training vocabulary, so it has no learned vector and
    /// can't be meaningfully scored (the classic item cold-start limit of
    /// collaborative filtering — brand-new books need a different path,
    /// e.g. a "new arrivals" list, until someone downloads them once).
    /// </summary>
    public List<ScoredBook> ScoreCandidates(ITransformer model, List<DownloadAnalysis> candidates)
    {
        if (candidates.Count == 0)
            return new List<ScoredBook>();

        var candidateData = _mlContext.Data.LoadFromEnumerable(candidates);
        var scored = model.Transform(candidateData);

        return _mlContext.Data
            .CreateEnumerable<ScoredCandidate>(scored, reuseRowObject: false)
            .Select(r => new ScoredBook(r.UserId, r.BookId, r.Score))
            .ToList();
    }
}
