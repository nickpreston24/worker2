using System.Threading.Channels;

namespace worker2;

public sealed class FileSystemQueue
{
    private readonly Channel<FileSystemEventArgs> _channel;

    public FileSystemQueue()
    {
        _channel = Channel.CreateBounded<FileSystemEventArgs>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        });
    }

    public async Task Produce(FileSystemEventArgs @event)
    {
        await _channel.Writer.WriteAsync(@event);
    }

    public async ValueTask<FileSystemEventArgs> Consume(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}