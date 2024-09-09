using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.PayOut;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Core.Services
{
    public class PayOutService : IPayOutService
    {
        private readonly ApiOptions _apiOptions;
        private readonly IHttpService _httpService;
        private readonly ILogger<PayOutService> _log;
        private readonly ICyberPayProvider _cyberPayProvider;
        private readonly IConfiguration _configuration;

        public PayOutService(ApiOptions apiOptions,
            IHttpService httpService,
            ILogger<PayOutService> log,
            ICyberPayProvider cyberPayProvider,
            IConfiguration configuration)
        {
            _apiOptions = apiOptions;
            _httpService = httpService;
            _log = log;
            _cyberPayProvider = cyberPayProvider;
            _configuration = configuration;

        }

        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest request)
        {
            try
            {
                var credentials = await _cyberPayProvider.GetClientCredentials();

                var url = $"{_apiOptions.CyberPayFundTransferUrl}/api/v1/account/name-enquiry";

                var stringContent = JsonConvert.SerializeObject(request);
                HttpContent httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                var nameEnquiryResponse = await _httpService.Post<NameEnquiryResponse>(url, httpContent, credentials.access_token);

                if (nameEnquiryResponse != null)
                {
                    _log.LogInformation($"Name Enquiry Response: {JsonConvert.SerializeObject(nameEnquiryResponse)}");
                    return nameEnquiryResponse;
                }
                else
                {
                    _log.LogInformation("Name Enquiry Response: null");
                    throw new Exception("Name enquiry response failed.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while checking name enquiry");
                throw new NotSuccessfulException("Failed to retrieve account name.");
            }
        }

        public async Task<InstantPayOutResponse> InstantPayOut(InstantPayOutRequest request, string merchantReference)
        {
            try
            {
                var MerchantRef = merchantReference;

                var nameEnquiryRequest = new NameEnquiryRequest
                {
                    AccountNumber = request.AccountNumber,
                    BankCode = request.BankCode,
                };

                var nameEnquiryResponse = await NameEnquiry(nameEnquiryRequest);

                if (nameEnquiryResponse == null || string.IsNullOrEmpty(nameEnquiryResponse.AccountName))
                {
                    _log.LogInformation($"Name Enquiry failed. Cannot proceed with instant payout: {JsonConvert.SerializeObject(nameEnquiryResponse)}");
                    throw new NotSuccessfulException($"Failed to retrieve account name for instant payout: {JsonConvert.SerializeObject(nameEnquiryResponse)}");
                }

                var credentials = await _cyberPayProvider.GetClientCredentials();
                var url = $"{_apiOptions.CyberPayFundTransferUrl}/instant";

                var instantPayOut = new
                {
                    
                    request.AccountNumber,
                    request.SenderName,
                    request.BeneficiaryName,
                   // request.BeneficiaryAccountNumber,
                    request.Amount,
                    request.Narration,
                   // request.PhoneNumber,
                    MerchantRef = merchantReference,
                    WalletCode = _configuration["ApiOptions:Zikora:WalletCode"],
                    WebHook = _configuration["ApiOptions:Zikora:WebHook"],
                    WalletType = _configuration["ApiOptions:Zikora:WalletType"],
                    MerchantCharge = _configuration["ApiOptions:Zikora:MerchantCharge"],
                    BankCode = _configuration["ApiOptions:Zikora:BankCode"],
                };

                var stringContent = JsonConvert.SerializeObject(instantPayOut);
                HttpContent httpContent = new StringContent(stringContent, Encoding.UTF8, "application/json");

                _log.LogInformation($"InstantPayOut Url: {url}");
                _log.LogInformation($"InstantPayOut Request Body: {stringContent}");

                var instantPayOutResponse = await _httpService.Post<InstantPayOutResponse>(url, httpContent, credentials.access_token);

                if (instantPayOutResponse != null)
                {
                    _log.LogInformation($"Instant PayOut Response: {JsonConvert.SerializeObject(instantPayOutResponse)}");
                    return instantPayOutResponse;
                }
                else
                {
                    _log.LogInformation($"Instant PayOut Response: {instantPayOutResponse.Message}");
                    throw new NotSuccessfulException($"Instant payout failed: {instantPayOutResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"An error occurred during instant payout: {ex.Message}");
                throw new OperationFailedException("Failed to complete instant payout.", ex);
            }
        }

        public async Task<RequeryResponse> RequeryPayOut(string reference)
        {
            try
            {
                var credentials = await _cyberPayProvider.GetClientCredentials();
                var url = $"{_apiOptions.CyberPayFundTransferUrl}/reference/{reference}";
                var response = await _httpService.Get<RequeryResponse>(url, credentials.access_token);

                if (response == null || !response.Succeeded)
                {
                    _log.LogError($"Failed to retrieve requery response. HTTP status code: {response?.Code ?? "null"}");
                    return null;
                }

                _log.LogInformation($"Requery Response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.InnerException?.Message ?? ex.Message);
                throw new NotSuccessfulException("Failed to complete requery.");
                
            }
        }

        public async Task<BankResponse> Get()
        {
            try
            {
                var credentials = await _cyberPayProvider.GetClientCredentials();
                var url = $"{_apiOptions.PaymentUrl}/api/v1/banks/all";
                var response = await _httpService.Get<BankResponse>(url, credentials.access_token);

                if (response == null || response.Data == null || !response.Data.Any())
                {
                    _log.LogError($"Failed to retrieve banks. HTTP status code: No valid response received.");
                    return null;
                }

                _log.LogInformation($"All Banks Response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.InnerException?.Message ?? ex.Message);
                throw new NotSuccessfulException("Failed to retrieve banks.");
            }
        }

    }
}
