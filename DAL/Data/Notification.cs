using DAL.Data.AuthModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data
{
    public class Notification
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public Guid App_userId { get; set; }
        public App_user? App_user { get; set; }

    }
}
