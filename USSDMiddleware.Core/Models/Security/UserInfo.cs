using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Models.IdentityModel;

namespace USSDMiddleware.Core.Models.Security
{
    public class UserInfo
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class TwoFactorAuthenticateModel
    {
        public string Id { get; set; }
        public string Otp { get; set; }
        public bool RememberMe { get; set; }
        public TokenModel[] Token { get; set; }
    }
}
