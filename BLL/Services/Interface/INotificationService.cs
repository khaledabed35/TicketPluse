using DAL.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface INotificationService
    {
        Task<bool> SendNotificationAsync(Guid userId, string title, string message);

        Task<bool> BroadcastNotificationAsync(string title, string message);

        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);

        Task<bool> MarkAsReadAsync(int notificationId, Guid userId);

        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(int notificationId, Guid userId);

    }
}
