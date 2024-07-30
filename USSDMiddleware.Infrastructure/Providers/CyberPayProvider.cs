using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Bills;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Infrastructure.Providers
{
    public class CyberPayProvider : ICyberPayProvider
    {
        private readonly ApiOptions _apiOptions;

        private readonly IHttpService _httpService;
        private readonly ILogger<CyberPayProvider> _log;
        private readonly IDistributedCache _distributedCache;


        private const string tokenKey = "CYBER_PAY_PAYOUT_TOKEN_KEY";

        public CyberPayProvider(ApiOptions apiOptions, IHttpService httpService, ILogger<CyberPayProvider> log, 
            IDistributedCache distributedCache)
        {
            _apiOptions = apiOptions;
            _httpService = httpService;
            _log = log;
            _distributedCache = distributedCache;
        }

        public async Task<CategoriesResponse> GetCategories(string categoryType)
        {
            try
            {
                CyberPayPayoutAuthResponse authResponse = await GetClientCredentials();
                string token = authResponse != null ? authResponse.access_token : string.Empty;

                CategoriesResponse transferResponse = await _httpService.Get<CategoriesResponse>($"{_apiOptions.CyberPayBillUrl}/api/v1/bill/category?categoryType={categoryType}", token);
                return transferResponse;
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            }

            return null;
        }

        public async Task<BillersResponse> GetBillers(string categoryId)
        {
            try
            {
                CyberPayPayoutAuthResponse authResponse = await GetClientCredentials();
                string token = authResponse != null ? authResponse.access_token : string.Empty;

                BillersResponse transferResponse = await _httpService.Get<BillersResponse>($"{_apiOptions.CyberPayBillUrl}/api/v1/bill/billers?categoryId={categoryId}", token);
                return transferResponse;
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            }

            return null;
        }

        public async Task<PaymentItemsResponse> GetPaymentItems(string billerId)
        {
            try
            {
                CyberPayPayoutAuthResponse authResponse = await GetClientCredentials();
                string token = authResponse != null ? authResponse.access_token : string.Empty;

                PaymentItemsResponse transferResponse = await _httpService.Get<PaymentItemsResponse>($"{_apiOptions.CyberPayBillUrl}/api/v1/bill/payment-items?billerId={billerId}", token);
                return transferResponse;
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            }

            return null;
        }

        public async Task<VendResponse> Vend(VendRequest requestModel)
        {
            try
            {
                var request = JsonConvert.SerializeObject(requestModel);
                HttpContent httpContent = new StringContent(request, Encoding.UTF8, "application/json");
                CyberPayPayoutAuthResponse authResponse = await GetClientCredentials();
                string token = authResponse != null ? authResponse.access_token : string.Empty;

                VendResponse transferResponse = await _httpService.Post<VendResponse>($"{_apiOptions.CyberPayBillUrl}/api/v1/vend", httpContent, token);
                return transferResponse;
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            };

            return null;
        }



        public async Task<ValidateResponse> Validate(ValidateRequest requestModel)
        {
            try
            {
                var request = JsonConvert.SerializeObject(requestModel);
                HttpContent httpContent = new StringContent(request, Encoding.UTF8, "application/json");
                CyberPayPayoutAuthResponse authResponse = await GetClientCredentials();
                string token = authResponse != null ? authResponse.access_token : string.Empty;

                ValidateResponse response = await _httpService.Post<ValidateResponse>($"{_apiOptions.CyberPayBillUrl}/api/v1/validate", httpContent, token);
                return response;
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            };

            return null;
        }

        public async Task<CyberPayPayoutAuthResponse> GetClientCredentials()
        {
            CyberPayPayoutAuthResponse token = RetrieveTokenFromCache();
            if (token == null)
            {
                return await GetFreshToken();
            }

            return token;
        }

        private CyberPayPayoutAuthResponse RetrieveTokenFromCache()
        {
            var dataStr = _distributedCache.GetString(tokenKey);
            if (dataStr != null)
            {
                CyberPayPayoutAuthResponse data;
                try
                {
                    data = JsonConvert.DeserializeObject<CyberPayPayoutAuthResponse>(dataStr);
                }
                catch (Exception)
                {
                    return null;
                }
                return data;
            }

            return null;
        }

        private async Task<CyberPayPayoutAuthResponse> GetFreshToken()
        {
            string authUrl = $"{_apiOptions.CyberPayAuthUrl}/oauth2/token?grant_type=client_credentials&scopes=profile";

            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(GetBasicAuth()))
                {
                    client.DefaultRequestHeaders.Add("Authorization", GetBasicAuth());
                }
                try
                {
                    HttpResponseMessage httpResponseMessage = await client.PostAsync(authUrl, null);
                    string response = httpResponseMessage.Content.ReadAsStringAsync().Result;

                    _log.LogInformation($"Response Body: {response}");

                    var freshToken = JsonConvert.DeserializeObject<CyberPayPayoutAuthResponse>(response);

                    if (freshToken == null) return null;

                    SaveTokenInCache(freshToken);

                    return freshToken;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                    _log.LogInformation($"Payout token error: {errorMsg}");

                    return null;
                }
            };
        }

        private void SaveTokenInCache(CyberPayPayoutAuthResponse data)
        {
            var options = new DistributedCacheEntryOptions()
            { AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(data.expires_in - 60) }; //Add expiry time from the token minua 1 minute

            var dataStr = JsonConvert.SerializeObject(data);
            _distributedCache.SetString(tokenKey, dataStr, options);
        }

        private string GetBasicAuth()
        {
            //convert username and password to byte array
            byte[] authBytes = Encoding.ASCII.GetBytes($"{_apiOptions.AuthUsername}:{_apiOptions.AuthPassword}");

            //convert to base64 string
            string basic = Convert.ToBase64String(authBytes);

            return $"Basic {basic}";
        }
    }
}
