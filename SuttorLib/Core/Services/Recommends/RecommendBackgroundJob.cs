namespace LibrarySystem.Recommendations.Services
{
    /// <summary>
    /// Runs RecommendationPipelineService.RunAsync() on a timer instead of
    /// waiting for someone to call POST /api/recommendations/train.
    ///
    /// BackgroundService is registered as a singleton by AddHostedService, but
    /// RecommendationPipelineService (and IUnitOfWork underneath it) is scoped —
    /// injecting it straight into this class's constructor would make it a
    /// "captive dependency": one instance reused for the app's entire lifetime,
    /// which for something wrapping a DbContext causes real problems (stale
    /// state, thread-safety issues since a DbContext isn't safe for concurrent
    /// use). So instead this creates a new DI scope for every run and resolves
    /// the pipeline fresh each time, exactly as a real HTTP request would.
    /// </summary>
    public class RecommendationBackgroundJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecommendationBackgroundJob> _logger;
        private readonly TimeSpan _interval;

        public RecommendationBackgroundJob(
            IServiceScopeFactory scopeFactory,
            ILogger<RecommendationBackgroundJob> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var hours = configuration.GetValue<double?>("Recommendations:IntervalHours") ?? 24;
            _interval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var pipeline = scope.ServiceProvider.GetRequiredService<RecommendationPipelineService>();

                    _logger.LogInformation("Recommendation pipeline run starting.");
                    await pipeline.RunAsync();
                    _logger.LogInformation("Recommendation pipeline run finished.");
                }
                catch (Exception ex)
                {
                    // A failed run (e.g. transient DB/Mongo issue) shouldn't kill
                    // the whole background loop — log it and try again next interval.
                    _logger.LogError(ex, "Recommendation pipeline run failed.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected when the app is shutting down.
                }
            }
        }
    }
}
