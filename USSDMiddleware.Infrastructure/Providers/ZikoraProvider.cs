using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Utilities;

namespace USSDMiddleware.Infrastructure.Providers
{
    public class ZikoraProvider : IUssdProvider
    {
        private readonly ApiOptions _apiOptions;

        private readonly IHttpService _httpService;
        private readonly ILogger<ZikoraProvider> _log;

        public Core.Enums.Providers ProviderType => Core.Enums.Providers.ZIKORA;

        public ZikoraProvider(ApiOptions apiOptions, IHttpService httpService,
            ILogger<ZikoraProvider> log)
        {
            _apiOptions = apiOptions;
            _httpService = httpService;
            _log = log;
        }

        public async Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request)
        {
            try
            {
                var url = $"{BuildUrl(RequestType.PhoneValidation)}&phoneNumber={request.PhoneNumber}";
                var serviceRsp = await _httpService.Get(url, BuildHeader());
                if (serviceRsp is { HasValue: true, Value: "true" })
                {
                    return new PhoneValidationResponse(false, true, "Phone number exists!");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to validate customer phone!");
            }

            return new PhoneValidationResponse(false, false, "Phone number does not exist!");
        }


        public async Task<AccountCreationResponse> CreateAccount(AccountCreationRequest req)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(req);
                var serviceResponse =
                    await _httpService.Post(BuildUrl(RequestType.CreateAccount), BuildHeader(), jsonContent);

                if (serviceResponse.HasValue)
                {
                    var rsp = JsonConvert.DeserializeObject<JObject>(serviceResponse.Value);
                    if (rsp != null)
                    {
                        var isSuccess = rsp["IsSuccessful"]!.Value<bool>();
                        if (!isSuccess)
                        {
                            throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED,
                                rsp["Message"]!.Value<string>());
                        }

                        var messageToken = rsp["Message"];
                        if (messageToken != null)
                        {
                            return BuildSuccessfulAccountResponse(messageToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to create account!");
            }

            throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Account creation failed.");
        }


        public async Task<BvnInfoResponse> GetBvnInfo(string bvn, string phoneNo)
        {
            var request = new Dictionary<string, string>
            {
                { "BVN", bvn },
                { "Token", _apiOptions.Zikora.Token }
            };

            var url = $"{_apiOptions.Zikora.BaseUrl}/Account/BVN/GetBVNDetails";
            var jsonContent = JsonConvert.SerializeObject(request);
            var serviceRsp = await _httpService.Post(url, BuildHeader(), jsonContent);
            if (serviceRsp.HasValue)
            {
                var bvnInfoResponse = JsonConvert.DeserializeObject<BvnInfoResponse>(serviceRsp.Value)!;
                if (!bvnInfoResponse.isBvnValid)
                {
                    throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Bvn is invalid");
                }

                if (!bvnInfoResponse.bvnDetails.phoneNumber.Equals(phoneNo))
                {
                    throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST,
                        "Bvn does not belong to this phone number!");
                }
            }

            throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED,
                "An unable to validate your bvn at the moment, try again later!");
        }


        
        private IDictionary<string, string> BuildHeader()
        {
            return null!;
        }


        private string BuildUrl(RequestType requestType)
        {
            var token = _apiOptions.Zikora.Token;
            var baseUrl = _apiOptions.Zikora.BaseUrl;
            switch (requestType)
            {
                case RequestType.PhoneValidation:
                    return $"{baseUrl}/BankOneWebAPI/api/Customer/PhoneNumberExist/2?authToken={token}";
                case RequestType.CreateAccount:
                    return $"{baseUrl}/api/Account/CreateAccountQuick/2?authToken={token}";
                default: return "";
            }
        }

        private static AccountCreationResponse BuildSuccessfulAccountResponse(JToken messageToken)
        {
            return Builder<AccountCreationResponse>.CreateNew()
                .With(a => a.CustomerId = messageToken.Contains("CustomerIDInString")
                    ? messageToken["CustomerIDInString"]!.Value<string>()
                    : "")
                .With(a => a.Reference = messageToken.Contains("TransactionTrackingRef")
                    ? messageToken["TransactionTrackingRef"]!.Value<string>()
                    : "")
                .With(a => a.AccountNumber = messageToken.Contains("AccountNumber")
                    ? messageToken["AccountNumber"]!.Value<string>()
                    : "")
                .With(a => a.FullName = messageToken.Contains("FullName")
                    ? messageToken["FullName"]!.Value<string>()
                    : "")
                .Build();
        }
    }

}




    

