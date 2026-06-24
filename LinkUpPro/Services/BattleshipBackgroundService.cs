using LinkUpPro.Core.Application.Interfaces.IServices;

namespace LinkUpPro.Services
{
    public class BattleshipBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BattleshipBackgroundService> _logger;

        public BattleshipBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BattleshipBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Battleship BackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var battleshipService = scope.ServiceProvider.GetRequiredService<IBattleshipService>();
                    await battleshipService.CheckTimeoutsAsync();
                    _logger.LogInformation("Battleship timeout check completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking battleship timeouts.");
                }
            }
        }
    }
}
