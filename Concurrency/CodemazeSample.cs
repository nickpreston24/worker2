/*
 *
 * https://code-maze.com/csharp-concurrentqueue/
 */

namespace worker2.Concurrency;

using System.Collections.Concurrent;

public record Order
{
    public string Id { get; set; } = string.Empty;
}

public class OrderMessageBus
{
    private readonly ConcurrentQueue<Order> _queue = new();

    public int Count => _queue.Count;

    public void Add(Order? order)
    {
        ArgumentNullException.ThrowIfNull(order);

        _queue.Enqueue(order);
    }

    public bool Fetch(out Order? order) => _queue.TryDequeue(out order);
}

public class Consumer
{
    private readonly OrderMessageBus _messageBus;

    public Consumer(OrderMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public Task Process()
    {
        return Task.Run(() =>
        {
            while (_messageBus.Fetch(out var order))
            {
                Console.WriteLine($"ProcessId {Task.CurrentId} | Processing order {order.Id}");
                Thread.Sleep(200);
            }
        });
    }
}

public class Producer
{
    private readonly OrderMessageBus _messageBus;
    private readonly int _numberOfMessages;

    public Producer(OrderMessageBus messageBus, int numberOfMessages)
    {
        _messageBus = messageBus;
        _numberOfMessages = numberOfMessages;
    }

    public Task Produce()
    {
        Console.WriteLine("producing...");
        return Task.Run(() =>
        {
            for (int i = 0; i < _numberOfMessages; i++)
            {
                _messageBus.Add(new Order { Id = Guid.NewGuid().ToString() });
            }
        });
    }
}