using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data
{
    public class User : IdentityUser<Guid>
    {
        public string f_name { get; set; }
        public string l_name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     
        public List<Order> Orders { get; set; } = [];
    }
    

    }

