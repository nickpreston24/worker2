using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace worker2;

public class FileSystemWorker : BackgroundService
{
    private readonly ILogger<FileSystemWorker> _logger;
    private readonly FileSystemQueue _queue;
    private readonly Listener _listener;

    public FileSystemWorker(
        ILogger<FileSystemWorker> logger,
        FileSystemQueue queue,
        Listener listener
    )
    {
        _logger = logger;
        _queue = queue;
        _listener = listener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            var @event = await _queue.Consume(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            _logger.LogInformation(
                "[{date}] {fileName} arrived to worker.",
                $"{DateTime.Now:O}",
                @event.Name
            );
        }
    }
}
