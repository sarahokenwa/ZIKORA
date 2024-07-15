using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.Security
{
    public class TokenModel : Model
    {
        public string Token { get; set; }
        public TokenType Type { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Id { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }

    public enum TokenType { AccessToken, IdToken, RefreshToken }
    public class Token
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "exp")]
        public double ExpiresIn { get; set; }
        [JsonProperty(PropertyName = "fullName")]
        public string TokenType { get; set; }
    }
}
