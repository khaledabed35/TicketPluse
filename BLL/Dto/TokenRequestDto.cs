using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Dto
{
    public class TokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
