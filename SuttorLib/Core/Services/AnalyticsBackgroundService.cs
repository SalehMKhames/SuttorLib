using SuttorLib.Core.Interfaces;

namespace SuttorLib.Core.Services
{
    public class AnalyticsBackgroundService : BackgroundService
    {
        private readonly ILogger<AnalyticsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int DELAY_HOURS = 24;

        public AnalyticsBackgroundService(ILogger<AnalyticsBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Analytics Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Analytics Background Service is doing background work.");

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var statsService = scope.ServiceProvider.GetRequiredService<IStatisticsCalculator>();
                        var recsService = scope.ServiceProvider.GetRequiredService<IRecommendationEngine>();

                        await statsService.CalculateDailyStatisticsAsync();
                        await recsService.GenerateUserRecommendationsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing analytics data generation.");
                }

                await Task.Delay(TimeSpan.FromHours(DELAY_HOURS), stoppingToken);
            }

            _logger.LogInformation("Analytics Background Service is stopping.");
        }
    }
}