using System.Threading.Channels;

public interface INotificationQueue
{
    ValueTask QueueNotificationAsync(NotificationMessage message);
    ValueTask<NotificationMessage> DequeueAsync(CancellationToken cancellationToken);
}

public class NotificationMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NotificationQueue : INotificationQueue
{
    // بنستخدم Channel لأنها Thread-safe وسريعة جداً في الـ Memory
    private readonly Channel<NotificationMessage> _queue;

    public NotificationQueue()
    {
        // Unbounded يعني يشيل أي عدد من الرسائل بدون ليميت
        _queue = Channel.CreateUnbounded<NotificationMessage>();
    }

    // ميثود لرمي الإشعار في الطابور
    public async ValueTask QueueNotificationAsync(NotificationMessage message)
    {
        await _queue.Writer.WriteAsync(message);
    }

    // ميثود الـ Worker بيناديها عشان يسحب الرسالة القادمة
    public async ValueTask<NotificationMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}