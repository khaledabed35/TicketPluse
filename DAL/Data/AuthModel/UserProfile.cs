using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data.AuthModel
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string f_name { get; set; }
        public string l_name { get; set; }

        public string App_userId { get; set; }
        public App_user App_user { get; set; }
    }

}
