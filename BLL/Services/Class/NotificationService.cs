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

        // 👈 تم تعديل البارامتر لـ Guid
        public async Task<bool> SendNotificationAsync(Guid userId, string title, string message)
        {
            var notification = new Notification
            {
                App_userId = userId, // Guid تطابق مباشر مع
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification);
            return true;
        }

        public async Task<bool> BroadcastNotificationAsync(string title, string message)
        {
            try
            {
                var userIds = await _userManager.Users
                    .Select(u => u.Id)
                    .ToListAsync();

                if (userIds == null || !userIds.Any())
                {
                    Console.WriteLine("⚠️ WARNING: No users found in AspNetUsers table!");
                    return false;
                }

                foreach (var userId in userIds)
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
                }

                // الحفظ الفعلي
                await _notificationRepo.savechange();
                Console.WriteLine("✅ SUCCESS: Notifications saved successfully to DB!");
                return true;
            }
            catch (Exception ex)
            {
                // 🚨 هنا السحر! هيطبعلك الإيرور الحقيقي اللي مانع الحفظ في الـ Console
                Console.WriteLine("❌❌ ERROR IN BROADCAST: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("❌❌ INNER EXCEPTION: " + ex.InnerException.Message);
                }
                throw; // عشان يرمي الإيرور للـ Controller ونشوفه في الـ Postman
            }
        }

        // 👈 تم تعديل البارامتر لـ Guid
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            var allNotifications = await _notificationRepo.GetAllAsync();

            return allNotifications
                .Where(n => n.App_userId == userId) // Guid مقارنة Guid مع
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        // 👈 تم تعديل البارامتر لـ Guid
        public async Task<bool> MarkAsReadAsync(int notificationId, Guid userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null || notification.App_userId != userId)
                return false;

            notification.IsRead = true;
            _notificationRepo.Update(notification);
            
            return true;
        }

        // 👈 تم تعديل البارامتر لـ Guid
        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var allNotifications = await _notificationRepo.GetAllAsync();

            var userNotifications = allNotifications
                .Where(n => n.App_userId == userId && !n.IsRead)
                .ToList();

            foreach (var notification in userNotifications)
            {
                notification.IsRead = true;
                _notificationRepo.Update(notification);
                _notificationRepo.savechange() ;    
            }

            return true;
        }
    }
}