using DAL.Data.AuthModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
     public class UserProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string?Email  { get; set; }
    }
}
