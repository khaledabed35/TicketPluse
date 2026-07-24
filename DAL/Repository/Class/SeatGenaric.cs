using DAL.Data;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class SeatGenaric:GenaricRebo<Seat>,ISeatGenaric

    {
        public SeatGenaric(AppDbContext context):base(context)
        {
            
        }
    }
}
