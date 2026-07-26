using BLL.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class NotificationBackgroundWorker : BackgroundService
{
    private readonly INotificationQueue _notificationQueue;
    private readonly IServiceProvider _serviceProvider; 

    public NotificationBackgroundWorker(INotificationQueue notificationQueue, IServiceProvider serviceProvider)
    {
        _notificationQueue = notificationQueue;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var notification = await _notificationQueue.DequeueAsync(stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    await notificationService.BroadcastNotificationAsync(notification.Title, notification.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing background notification: {ex.Message}");
            }
        }
    }
}