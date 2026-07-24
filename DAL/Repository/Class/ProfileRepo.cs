using DAL.Data.AuthModel;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class ProfileRepo:GenaricRebo<UserProfile>,IProfileRepo
    {
        public ProfileRepo(AppDbContext context):base(context)
        {
            
        }
    }
}
