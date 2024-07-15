using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Models.Security;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Interfaces.Providers;

namespace USSDMiddleware.Core.Services
{
    public class IdentityService : IIdentityService
    {
        private IdentityOptions _options;
        private IHttpService _http;

        public IdentityService(IdentityOptions options,
            IHttpService http)
        {
            _options = options;
            _http = http;

        }

        public async Task<TokenModel> GetClientToken()
        {

            var payload = new
            {
                username = _options.ClientId,
                password = _options.Password,
                clienttype = _options.ClientType,
                clientcode = _options.ClientId
            };
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                var token = await client.PostAsync($"{_options.Authority}/api/v1/account", httpContent);
                string response = await token.Content.ReadAsStringAsync();
                var tokenString = JsonConvert.DeserializeObject<Response<Token>>(response);



                return new TokenModel
                {
                    ExpiryDate = DateTime.Now.AddSeconds(tokenString.Data.ExpiresIn),
                    Token = tokenString.Data.AccessToken,
                    Type = tokenString.Data.TokenType.ToEnum<TokenType>()
                };
            }
        }

        public async Task<IdentityUserModel> GetUser(string emailAddress)
        {
            var token = await GetClientToken();
            var url = $"{_options.Authority}/api/v1/users/{_options.ClientId}/{emailAddress}";
            var response = await _http.Get<Response<IdentityUserModel>>(url, token.Token);
            if (!response.Succeeded) throw new Exception(HttpStatusCode.BadRequest.ToString());
            return response.Data;
        }

        public async Task<IdentityUserModel> CreateUser(IdentityUserModel userModel)
        {
            var token = await GetClientToken();
            userModel.Validate();
            var payload = new
            {
                username = userModel.Email,
                email = userModel.Email,
                password = userModel.Password,
                confirmPassword = userModel.ConfirmPassword,
                lastName = userModel.FullName,
                otherNames = " ",
                clientCode = _options.ClientId,
            };
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
           

            try
            {
                var url = $"{_options.Authority}/api/v1/users";
                var response = await _http.Post<JObject>(url, httpContent, token.Token);
               
               

                var createUser = new Response<IdentityUserModel>
                {
                    Code = (string)response["code"],
                    Succeeded = (bool)response["succeeded"],
                    Message = (string)response["message"],
                    Data = new IdentityUserModel
                    {
                        Email = (string)response["data"]["username"],
                        UserId = (string)response["data"]["userId"],
                    }
                };

                  if (!createUser.Succeeded) throw new Exception(createUser.Message);
                return createUser.Data;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occur while trying to create User", ex);
            }
        }

        public async Task<string[]> GetPermissions()
        {
            var token = await GetClientToken();
            var httpResponse = new HttpResponseMessage();
            try
            {
                var url = $"{_options.Authority}/api/v1/permission/all";
                var response = await _http.Get<Response<PermissionModel[]>>(url, token.Token);
                return response.Data.Select(x => x.PermissionName).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IdentityUserModel> GetUserById(string userId)
        {
            var token = await GetClientToken();
            var url = $"{_options.Authority}/api/v1/users/{userId}";
            var response = await _http.Get<Response<IdentityUserModel>>(url, token.Token);
            if (!response.Succeeded) throw new Exception(response.Message);
            return response.Data;
        }

        public async Task<List<string>> GetUsersInPermission(string permissionName)
        {
            var token = await GetClientToken();
            var url = $"{_options.Authority}/api/v1/getUser/{permissionName}";
            var response = await _http.Get<Response<List<string>>>(url, token.Token);
            if (!response.Succeeded) throw new Exception(response.Message);
            return response.Data;
        }
  
    }

    public class IdentityOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string ClientType { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
