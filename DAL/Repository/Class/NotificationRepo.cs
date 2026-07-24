using DAL.Data;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class NotificationRepo:GenaricRebo<Notification>,INotificationRepo
    {
        public NotificationRepo(AppDbContext context):base(context)
        {
            
        }
    }
}
