using BLL.Services.Interface;
using DAL.Data;
using DAL.Data.AuthModel;
using DAL.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IGenaricRePo<Notification> _notificationRepo;
        private readonly UserManager<App_user> _userManager;

        public NotificationService(IGenaricRePo<Notification> notificationRepo, UserManager<App_user> userManager)
        {
            _notificationRepo = notificationRepo;
            _userManager = userManager;
        }

        public async Task<bool> SendNotificationAsync(Guid userId, string title, string message)
        {
            var notification = new Notification
            {
                App_userId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.savechange(); // ✅ تأكد من الحفظ هنا
            return true;
        }

        public async Task<bool> BroadcastNotificationAsync(string title, string message)
        {
            try
            {
                // سحب الـ IDs فقط بدون سحب كل بيانات اليوزر
                var userIds = await _userManager.Users
                    .Select(u => u.Id)
                    .ToListAsync();

                if (userIds == null || !userIds.Any())
                {
                    Console.WriteLine("⚠️ WARNING: No users found in AspNetUsers table!");
                    return false;
                }

                // ✅ الصح: بنعمل لستة ونضيف فيها كله في الـ Memory مرة واحدة
                var notificationsList = new List<Notification>();

                foreach (var userId in userIds)
                {
                    notificationsList.Add(new Notification
                    {
                        App_userId = userId,
                        Title = title,
                        Message = message,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }

                // ✅ تعديل جوهري: لو الـ Generic Repository عندك بيدعم AddRangeAsync استخدمه،
                // لو مش بيدعم، بنعمل AddAsync العادي بس برة اللوب مش جواه عشان الـ Tracking يحصل مرة واحدة.
                foreach (var notification in notificationsList)
                {
                    await _notificationRepo.AddAsync(notification);
                }

                // الحفظ الفعلي دفعة واحدة في الداتابيز
                await _notificationRepo.savechange();
                Console.WriteLine($"✅ SUCCESS: {notificationsList.Count} Notifications saved successfully to DB!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌❌ ERROR IN BROADCAST: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("❌❌ INNER EXCEPTION: " + ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            // 💡 نصيحة للمستقبل: يفضل استخدام Specification هنا بدل GetAllAsync عشان متسحبش كل إشعارات السيرفر للـ Memory
            var allNotifications = await _notificationRepo.GetAllAsync();

            return allNotifications
                .Where(n => n.App_userId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null || notification.App_userId != userId)
                return false;

            notification.IsRead = true;
            _notificationRepo.Update(notification);
            await _notificationRepo.savechange(); // ✅ حفظ التعديل

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var allNotifications = await _notificationRepo.GetAllAsync();

            var userNotifications = allNotifications
                .Where(n => n.App_userId == userId && !n.IsRead)
                .ToList();

            if (!userNotifications.Any()) return true;

            foreach (var notification in userNotifications)
            {
                notification.IsRead = true;
                _notificationRepo.Update(notification);
                // ❌ شيلنا الـ savechange من جوة اللوب هنا عشان ميعملش Database Roundtrip مع كل إشعار!
            }

            // ✅ الـ savechange بتناديها مرة واحدة بس بعد ما تخلص اللوب بالكامل
            await _notificationRepo.savechange();

            return true;
        }
        public async Task<bool> DeleteNotificationAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);

            if (notification == null || notification.App_userId != userId)
                return false;

            _notificationRepo.Delete(notification);
            await _notificationRepo.savechange();

            return true;
        }
    }
}