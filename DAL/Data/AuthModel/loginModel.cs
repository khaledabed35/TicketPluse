
using System.ComponentModel.DataAnnotations;

namespace DAL.Data.AuthModel
{
    public class loginModel
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
