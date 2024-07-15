using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class TokenModel : Model
    {
        public string Token { get; set; }
        public TokenType Type { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Id { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int ExpiresIn { get; set; }

    }

    public enum TokenType { AccessToken, IdToken, RefreshToken }
}
