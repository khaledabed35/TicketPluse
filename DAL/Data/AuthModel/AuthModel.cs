using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Data.AuthModel
{
   public class AuthModel
    {
        public string Message { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public List<string> role { get; set; }
        public string token { get; set; }
        public DateTime expireon  { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
