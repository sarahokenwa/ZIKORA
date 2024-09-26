using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Models.IdentityModel;
using USSDMiddleware.Core.Models;
using Microsoft.Extensions.Configuration;
using USSDMiddleware.Core.Models.Security;
using TokenModel = USSDMiddleware.Core.Models.Security.TokenModel;
using TokenType = USSDMiddleware.Core.Models.Security.TokenType;

namespace USSDMiddleware.Core.Services
{
    public class SecurityService
    {
        private readonly IdentityOptions _idOptions;

        private IHttpService _http;
        private readonly IConfiguration _configuration;

        public SecurityService(IdentityOptions idOptions, IHttpService http, IConfiguration configuration)
        {
            _idOptions = idOptions;
            _http = http;
            _configuration = configuration;
        }


        public async Task<TokenModel[]> GetTokens(UserInfo model)
        {
            Response<Dictionary<string, string>> response = await GetUserCredentials(model);

            var expiryDate = DateTime.Now.AddSeconds(double.Parse(response.Data["exp"]));

            var refreshToken = new TokenModel
            {
                Id = response.Data["id"],
                Type = TokenType.RefreshToken,
                Token = response.Data["refreshToken"],
                ExpiryDate = expiryDate,
                TwoFactorEnabled = Convert.ToBoolean(response.Data["twoFactorEnabled"])
            };

            var accessToken = new TokenModel
            {
                Id = response.Data["id"],
                Type = TokenType.AccessToken,
                ExpiryDate = expiryDate,
                Token = response.Data["accessToken"],
                TwoFactorEnabled = Convert.ToBoolean(response.Data["twoFactorEnabled"])
            };

            return new[] { accessToken, refreshToken };
        }

        public async Task<Response<Dictionary<string, string>>> GetUserCredentials(UserInfo model)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                UserName = model.Email,
                Password = model.Password,
                ClientType = "Client",
                ClientCode = _configuration["Identity:ClientId"]
            });


            string responseString = await Post($"{_configuration["Identity:Authority"]}/api/v1/account", payload);


            var response = JsonConvert.DeserializeObject<Response<Dictionary<string, string>>>(responseString);

            if (response.Code == "IDS00")
            {
                return response;
            }
            var error = new Exception(response.Message);

            error.Data.Add("error", responseString);


            throw error;

        }

        private async Task<string> Post(string resourceUrl, string payload, string token = null)
        {

            HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(2);

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                try
                {
                    HttpResponseMessage httpResponseMessage = await client.PostAsync(resourceUrl, httpContent);
                    string response = await httpResponseMessage.Content.ReadAsStringAsync();
                    return response;
                }
                catch (Exception e)
                {
                    string errMsg = e.InnerException != null ? e.InnerException.Message : e.Message;

                    return null;
                }
            };
        }

    }


}
