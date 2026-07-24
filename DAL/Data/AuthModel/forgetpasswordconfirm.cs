

namespace DAL.Data.AuthModel
{
    public class forgetpasswordconfirm
    {
        public string userid { get; set; }
        public string token { get; set; }
        public string newpassword { get; set; }
        public string confirmpassword { get; set; }
        public string email { get; set; }
    }
}
