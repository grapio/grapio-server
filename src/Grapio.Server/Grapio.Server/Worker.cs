using FASTER.server;

namespace Grapio.Server;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var options = new ServerOptions
            {
                logger = _logger,
                Address = "127.0.0.1",
                CheckpointDir = "data",
                DisablePubSub = true,
                EnableStorageTier = false,
                LogDir = "logs",
                Port = 3278
            };

            using var server = new VarLenServer(options);
            server.Start();

            await Task.Delay(1000, stoppingToken);
        }
    }
}