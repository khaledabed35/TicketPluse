using DAL.Data;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class EventRepo:GenaricRebo<Event>,IEventRePo
    {
        public EventRepo(AppDbContext context):base(context)
        {
            
        }
    }
}
