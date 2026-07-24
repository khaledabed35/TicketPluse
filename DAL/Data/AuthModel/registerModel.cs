
using System.ComponentModel.DataAnnotations;

namespace DAL.Data.AuthModel
{
    public class registerModel

    {
        [Required]
        public string Email { get; set; }
        [Required]

        public string firstname { get; set; }
        [Required]

        public string lastname { get; set; }
        [Required]

        public string password { get; set; }
        [Required]

        public string username { get; set; }
    }
}
