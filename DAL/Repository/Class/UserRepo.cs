using DAL.Data.AuthModel;
using DAL.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repository.Class
{
    public class UserRepo:GenaricRebo<App_user>,IUserRepo

    {
        public UserRepo(AppDbContext context):base(context)
        {
            
        }
    }
}
