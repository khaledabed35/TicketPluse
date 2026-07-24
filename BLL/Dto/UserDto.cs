using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string PhoneNumber { get; set; }
    }
}
