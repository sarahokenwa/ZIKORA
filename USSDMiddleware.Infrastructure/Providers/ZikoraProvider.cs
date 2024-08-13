using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;

namespace USSDMiddleware.Infrastructure.Providers
{
    public class ZikoraProvider : IUssdProvider
    {
        private readonly ApiOptions _apiOptions;

        private readonly IHttpService _httpService;
        private readonly ILogger<ZikoraProvider> _log;
        private readonly IConfiguration _configuration;


        public Core.Enums.Providers ProviderType => Core.Enums.Providers.ZIKORA;

        public ZikoraProvider(ApiOptions apiOptions, IHttpService httpService,
            ILogger<ZikoraProvider> log, IConfiguration configuration)
        {
            _apiOptions = apiOptions;
            _httpService = httpService;
            _log = log;
            _configuration = configuration;
        }

        public async Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request)
        {
            try
            {
                string url =  $"{BuildUrl("/BankOneWebAPI/api/Customer/PhoneNumberExist/2")}&phoneNumber={request.PhoneNumber}";
                var boolRsp = await _httpService.Get<bool>(url, BuildHeader());
                if (boolRsp.Equals(true))
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
        
        public async Task<GetUserByPhoneNumberResponse> GetUserByPhoneNumber(string phoneNumber)
        {
            try
            {
                var url = $"{BuildUrl("/BankOneWebAPI/api/Customer/GetByCustomerPhoneNumber/2")}&phoneNumber={phoneNumber}";
                var users = await _httpService.Get<ZikoraGetUserByPhoneNumberResponse[]>(url, BuildHeader());

                if (users == null || users.Length == 0)
                {
                    _log.LogError("No customer found in the response");
                    throw new NotFoundException("Failed to retrieve user.");
                }

                var user = users[0];
                return Builder<GetUserByPhoneNumberResponse>.CreateNew()
                    .With(g => g.PhoneNumber = user.PhoneNumber)
                    .With(g => g.Address = user.Address)
                    .With(g => g.Email = user.Email)
                    .With(g => g.CustomerID = user.CustomerID)
                    .With(g => g.LastName = user.LastName)
                    .With(g => g.OtherNames = user.OtherNames)
                    .With(g => g.BankVerificationNumber = user.BankVerificationNumber)
                    .With(g => g.DateOfBirth = user.DateOfBirth)
                    .Build();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while processing accounts retrieval");
                throw new NotSuccessfulException("Failed to retrieve users.");
            }
        }


        public async Task<AccountCreationResponse> CreateAccount(AccountCreationRequest req)
        {
            try
            {
                var url = BuildUrl($"BankOneWebAPI/api/Account/CreateAccountQuick/2");
                var rsp = await _httpService.Post<JObject>(url, BuildHeader(), JsonConvert.SerializeObject(req));
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

            var jsonContent = JsonConvert.SerializeObject(request);
            var bvnInfoResponse = await _httpService.Post<BvnInfoResponse>(BuildUrl("thirdpartyapiservice/apiservice/Account/BVN/GetBVNDetails"), BuildHeader(), jsonContent);
            if (!bvnInfoResponse.isBvnValid)
            {
                throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Bvn is invalid");
            }

            if (!bvnInfoResponse.bvnDetails.phoneNumber.Equals(phoneNo))
            {
                throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST,
                    "Bvn does not belong to this phone number!");
            }
            if (!bvnInfoResponse.RequestStatus)
            {

                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED,
                    "An unable to validate your bvn at the moment, try again later!");
            }
            return bvnInfoResponse;
        }

        public async Task<string> GetProviderId(IProviderManager providerManager)
        {
            var provider = await providerManager.GetProviderByName(Core.Enums.Providers.ZIKORA.ToString());
            return provider.Id;
        }

        public async Task<BalanceEnquiryResponse> CheckAccountBalance(BalanceEnquiryRequest model)
        {
            var computeWithDrawableBalance = true;
            var url = $"{BuildUrl("/BankOneWebAPI/api/Account/GetAccountByAccountNumber/2")}&accountNumber={model.AccountNumber}" +
                      $"&computeWithDrawableBalance={computeWithDrawableBalance}&provider={Core.Enums.Providers.ZIKORA}";
            try
            {
                var serviceRsp = await _httpService.Get<ZikoraBalanceEnquiryResponse>(url, BuildHeader());
                return Builder<BalanceEnquiryResponse>.CreateNew()
                    .With(b => b.AvailableBalance = serviceRsp.AvailableBalance)
                    .With(b => b.LedgerBalance = serviceRsp.LedgerBalance)
                    .With(b => b.WithdrawableBalance = serviceRsp.WithdrawableBalance)
                    .With(b => b.AccountType = serviceRsp.AccountType)
                    .Build();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while checking account balance");
                throw new NotSuccessfulException("Failed to retrieve account balance");
            }
        }
        
        public async Task<List<GetAccountResponse>> GetAccountsByPhoneNumber(string phoneNumber)
        {
            try
            {
                var url = $"{BuildUrl("/BankOneWebAPI/api/Customer/GetByCustomerPhoneNumber/2")}&&phoneNumber={phoneNumber}";
                var serviceRsp = await _httpService.Get<JArray>(url, BuildHeader());

                if (serviceRsp == null || serviceRsp.Count == 0)
                {
                    _log.LogError("No accounts found in the response");
                    throw new NotSuccessfulException("Failed to retrieve accounts.");
                }

                var accounts = serviceRsp.SelectMany(customer => customer["Accounts"])
                    .Select(account => new GetAccountResponse
                    {
                        AccountNumber = account["AccountNumber"]?.ToString(),
                        AccountType = account["AccountType"]?.ToString(),
                        AccountStatus = account["AccountStatus"]?.ToString(),
                        AccessLevel = account["AccessLevel"]?.ToString()
                    })
                    .ToList();
                
                return accounts;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while processing accounts retrieval");
                throw new NotSuccessfulException("Failed to retrieve accounts.");
            }
        }

        public async Task<DebitCustomerAccountResponse> DebitCustomerAccount(DebitCustomerAccountRequest model)
        {
            try
            {
                var authenticationToken = _apiOptions.Zikora.Token;
                var debitUrl = $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/CoreTransactions/Debit";
               
                var headers = BuildHeader();

                var debitPayload = new
                {
                    model.RetrievalReference,
                    model.AccountNumber,
                    model.Amount,
                    model.Narration,
                    GLCode = _configuration["ApiOptions:GLCode"],
                    NibssCode = _configuration["ApiOptions:NibssCode"],
                    Fee = _configuration["ApiOptions:FundTransferFee"],
                    token = authenticationToken,
                };

                var jsonContent = JsonConvert.SerializeObject(debitPayload);
                var debitResponseContent = await _httpService.Post<DebitCustomerAccountResponse>(debitUrl, headers, jsonContent);

                if (debitResponseContent != null)
                {

                    var debitResult = new DebitCustomerAccountResponse
                    {
                        Succeeded = debitResponseContent.Succeeded,
                        Message = debitResponseContent.Message,
                        Code = debitResponseContent.Code,
                        Data = new DebitCustomerAccountResponse.ResponseData
                        {
                            IsSuccessful = debitResponseContent.Data.IsSuccessful,
                            ResponseMessage = debitResponseContent.Data.ResponseMessage,
                            ResponseCode = debitResponseContent.Data.ResponseCode,
                            Reference = debitResponseContent.Data.Reference
                        }
                    };

                    _log.LogInformation($"Debit result: {JsonConvert.SerializeObject(debitResult)}");

                    if (!debitResult.Data.IsSuccessful)
                    {
                        _log.LogError($"Debit was not successful: {debitResult.Data.ResponseMessage}");
                        throw new NotSuccessfulException($"Failed to debit customer account: {debitResult.Data.ResponseMessage}");
                    }


                    _log.LogInformation($"Debit result: {JsonConvert.SerializeObject(debitResult)}");

                    if (!debitResult.Data.IsSuccessful)
                    {
                        _log.LogError("Debit was not successful: {debitResult.ResponseMessage}");
                        throw new NotSuccessfulException("Failed to debit customer account: {debitResult.ResponseMessage}");
                    }
                    return debitResult;

                }
                else
                {
                    _log.LogError("Failed to debit customer account: {debitResult.ResponseMessage}");
                    throw new NotSuccessfulException("Failed to debit customer account: {debitResult.ResponseMessage}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while debiting customer account: {debitResult.ResponseMessage}");
                throw new NotSuccessfulException("Failed to debit customer account: {debitResult.ResponseMessage}");
            }
        }

        public async Task<RequeryResponse> StatusQuery(ReQueryRequest model)
        {
            try
            {
                var authenticationToken = _apiOptions.Zikora.Token;
                var statusQueryUrl = $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/CoreTransactions/TransactionStatusQuery";

                var headers = BuildHeader();

                var statusQueryPayload = new
                {
                    model.RetrievalReference,
                    TransactionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    TransactionType = "DEBIT",
                    model.Amount,
                    token = authenticationToken,
                };

                var statusQueryJsonContent = JsonConvert.SerializeObject(statusQueryPayload);

                _log.LogInformation($"Status query payload: {statusQueryJsonContent}");

                var statusQueryResponseContent = await _httpService.Post<RequeryResponse>(statusQueryUrl, headers, statusQueryJsonContent);

                if (statusQueryResponseContent != null)
                {
                    var statusQueryResult = new RequeryResponse
                    {
                        Code = statusQueryResponseContent.Code,
                        Succeeded = statusQueryResponseContent.Succeeded,
                    };

                    return statusQueryResult;
                }
                else
                {
                    _log.LogError("Failed to query transaction status: {statusQueryResult.Succeded} ");
                    throw new NotSuccessfulException("Failed to query transaction status: {statusQueryResult.Succeded}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while querying transaction status: {statusQueryResult.Code}");
                throw new NotSuccessfulException("Failed to query transaction status: {statusQueryResult.Succeded}");
            }
        }
        /** public async Task<IntraBankTransferResponse> IntraBankTransfer(IntraBankTransferRequest model)
         {
             try
             {
                 var authenticationKey = _apiOptions.Zikora.Token;

                 var transferUrl =
                     $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/CoreTransactions/LocalFundsTransfer";
                 var statusQueryUrl =
                     $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/CoreTransactions/TransactionStatusQuery";

                 var headers = BuildHeader();

                 var transferPayload = new
                 {
                     model.FromAccountNumber,
                     model.ToAccountNumber,
                     model.Fee,
                     model.RetrievalReference,
                     model.Narration,
                     model.Amount,
                     AuthenticationKey = authenticationKey
                 };

                 var jsonContent = JsonConvert.SerializeObject(transferPayload);

                 var transferResponseContent = await _httpService.Post(transferUrl, headers, jsonContent);

                 if (!string.IsNullOrEmpty(transferResponseContent))
                 {
                     var transferResponseJson = JsonConvert.DeserializeObject<JObject>(transferResponseContent);
                     _log.LogInformation($"Transfer response content: {transferResponseJson}");

                     var transferResult = new IntraBankTransferResponse
                     {
                         IsSuccessful = transferResponseJson["IsSuccessful"].Value<bool>(),
                         ResponseMessage = transferResponseJson["ResponseMessage"]?.ToString(),
                         ResponseCode = transferResponseJson["ResponseCode"]?.ToString(),
                         Reference = transferResponseJson["Reference"]?.ToString()
                     };

                     if (!transferResult.IsSuccessful)
                     {
                         _log.LogError("Transfer was not successful.");
                         throw new NotSuccessfulException("Failed to perform intra-bank transfer.");
                     }

                     var statusQueryPayload = new
                     {
                         RetrievalReference = model.RetrievalReference,
                         TransactionDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                         TransactionType = "CREDIT",
                         model.Amount,
                         token = authenticationKey,
                     };

                     var statusQueryJsonContent = JsonConvert.SerializeObject(statusQueryPayload);

                     var statusQueryResponseContent =
                         await _httpService.Post(statusQueryUrl, headers, statusQueryJsonContent);

                     if (!string.IsNullOrEmpty(statusQueryResponseContent))
                     {
                         var statusQueryResponseJson =
                             JsonConvert.DeserializeObject<JObject>(statusQueryResponseContent);
                         _log.LogInformation($"Status query response content: {statusQueryResponseJson}");

                         var statusQueryResult = new IntraBankTransferResponse
                         {
                             IsSuccessful = statusQueryResponseJson["IsSuccessful"].Value<bool>(),
                             ResponseMessage = statusQueryResponseJson["ResponseMessage"]?.ToString(),
                             ResponseCode = statusQueryResponseJson["ResponseCode"]?.ToString(),
                             Reference = transferResponseJson["Reference"]?.ToString()
                         };

                         return statusQueryResult;
                     }
                     else
                     {
                         _log.LogError("Failed to query transaction status.");
                         throw new NotSuccessfulException("Failed to query transaction status.");
                     }
                 }
                 else
                 {
                     _log.LogError("Failed to perform intra-bank transfer.");
                     throw new NotSuccessfulException("Failed to perform intra-bank transfer.");
                 }
             }
             catch (Exception ex)
             {
                 _log.LogError(ex, "An error occurred while performing intra-bank transfer.");
                 throw new NotSuccessfulException("Failed to perform intra-bank transfer.");
             }
         }

          public async Task<FreezeAccountResponse> FreezeAccount(FreezeAccountRequest model)
          {
              try
              {
                  var authenticationCode = _apiOptions.Zikora.Token;

                  var url = $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/Account/ActivatePND";

                  var headers = BuildHeader();

                  var freezeAccountPayload = new
                  {
                      model.AccountNo,
                      AuthenticationCode = authenticationCode,
                  };

                  var jsonContent = JsonConvert.SerializeObject(freezeAccountPayload);

                  var responseContent = await _httpService.Post(url, headers, jsonContent);

                  if (!string.IsNullOrEmpty(responseContent))
                  {
                      var responseJson = JsonConvert.DeserializeObject<JObject>(responseContent);
                      _log.LogInformation($"Response content: {responseContent}");

                      var blockAccount = new FreezeAccountResponse
                      {
                          RequestStatus = responseJson["RequestStatus"].Value<bool>(),
                          ResponseDescription = responseJson["ResponseDescription"]?.ToString(),
                          ResponseStatus = responseJson["ResponseStatus"]?.ToString(),
                      };

                      if (!blockAccount.RequestStatus)
                      {
                          _log.LogError("The attempt to freeze the account was not successful.");
                          throw new NotSuccessfulException("Failed to freeze account.");
                      }

                      return blockAccount;
                  }

                  else
                  {
                      _log.LogError("The attempt to freeze the account was not successful.");
                      throw new NotSuccessfulException("Failed to freeze account.");
                  }
              }
              catch (Exception ex)
              {
                  _log.LogError(ex, "An error occurred while freezing account");
                  throw new NotSuccessfulException("Failed to freeze account.");
              }
          }

           public async Task<GetCustomerCardResponse> GetCustomerCards(GetCustomerCardRequest request)
           {
               try
               {
                   var token = _apiOptions.Zikora.Token;
                   var includeInactiveCards = false;
                   var url = $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/Cards/RetrieveCustomerCards";

                   var headers = BuildHeader();

                   var customerCardsPayload = new
                   {
                       request.AccountNo,
                       Token = token,
                       IncludeInactiveCards = includeInactiveCards
                   };

                   var jsonContent = JsonConvert.SerializeObject(customerCardsPayload);

                   var responseContent = await _httpService.Post(url, headers, jsonContent);

                   if (!string.IsNullOrEmpty(responseContent))
                   {
                       var responseJson = JsonConvert.DeserializeObject<JObject>(responseContent);
                       _log.LogInformation($"Response content: {responseContent}");

                       var cardResponse = new GetCustomerCardResponse
                       {
                           isSuccessful = responseJson["isSuccessful"].Value<bool>(),
                           ResponseDescription = responseJson["ResponseDescription"]?.ToString(),
                           Cards = responseJson["Cards"]?.ToObject<List<CardModel>>(),
                       };

                       if (!cardResponse.isSuccessful)
                       {
                           _log.LogError("The attempt to retrieve cards was not successful.");
                           throw new NotSuccessfulException("Failed to retrieve card.");
                       }

                       return cardResponse;
                   }
                   else
                   {
                       _log.LogError("The attempt to retrieve cards was not successful.");
                       throw new NotSuccessfulException("Failed to retrieve cards.");
                   }
               }
               catch (Exception ex)
               {
                   _log.LogError(ex, "An error occurred while retreiving cards.");
                   throw new NotSuccessfulException("Failed to retrieve cards.");
               }
           }
           **/




        private IDictionary<string, string>? BuildHeader()
        {
            return null!;
        }


        private string BuildUrl(String url)
        {
            var token = _apiOptions.Zikora.Token;
            var baseUrl = _apiOptions.Zikora.BaseUrl;
            return $"{baseUrl}/{url}?authToken={token}";
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




    

