using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USSDMiddleware.Core.Models.IdentityModel
{
    public class Token
    {
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName = "exp")]
        public int ExpiresIn { get; set; }
        [JsonProperty(PropertyName = "fullName")]
        public string TokenType { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
