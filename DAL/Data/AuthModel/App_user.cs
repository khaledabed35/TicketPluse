using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DAL.Data.AuthModel
{
    public class App_user : IdentityUser<Guid>
    {
        public string f_name { get; set; }
        public string l_name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpireTime { get; set; }
    }
}