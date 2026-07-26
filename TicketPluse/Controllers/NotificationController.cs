using BLL.Services.Class;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduMangment.Controllers
{
    [Authorize] // حماية كاملة، لازم يكون مسجل دخول
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // 1. الحصول على إشعارات المستخدم الحالي
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 👈 1. تحويل الـ string لـ Guid لتطابق السيرفس
            var userGuid = Guid.Parse(userId);
            var notifications = await _notificationService.GetUserNotificationsAsync(userGuid);
            return Ok(notifications);
        }

        // 2. تحديث إشعار معين كـ مقروء
        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 👈 2. تحويل الـ string لـ Guid
            var userGuid = Guid.Parse(userId);
            var success = await _notificationService.MarkAsReadAsync(id, userGuid);
            if (!success) return BadRequest(new { Message = "Notification not found or access denied" });

            return Ok(new { Message = "Notification marked as read" });
        }

        // 3. تحديث كل الإشعارات كـ مقروءة
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 👈 3. تحويل الـ string لـ Guid
            var userGuid = Guid.Parse(userId);
            await _notificationService.MarkAllAsReadAsync(userGuid);
            return Ok(new { Message = "All notifications marked as read" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] int id)
        {
            // الترتيب الصح اللي شغال في بقية الـ Controller عندك: بنجيب الـ uid الأول
            var userIdClaim = User.FindFirst("uid")?.Value
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new { message = "User identity is invalid or missing." });
            }

            var result = await _notificationService.DeleteNotificationAsync(id, userId);

            if (!result)
            {
                return NotFound(new { message = "Notification not found or you don't have permission to delete it." });
            }

            return Ok(new { message = "Notification deleted successfully." });
        
    }
}
        
}