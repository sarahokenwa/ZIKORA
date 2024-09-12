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
using USSDMiddleware.Core.Models.Accounts;
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


                _log.LogInformation($"ValidatePhone Url: {url}");

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

                _log.LogInformation($"Get User By Phone number Url: {url}");

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

        public async Task<GetUserByAccountNumberResponse> GetUserByAccountNumber(string accountNumber)
        {
            try
            {
                var url = $"{BuildUrl("/BankOneWebAPI/api/Customer/GetByAccountNo2/2")}&accountNumber={accountNumber}";

                _log.LogInformation($"GetUserByAccountNumber Url: {url}");

                var user = await _httpService.Get<GetUserByAccountNumberResponse>(url, BuildHeader());

                if (user == null)
                {
                    _log.LogError("No customer found in the response");
                    throw new NotFoundException("Failed to retrieve user.");
                }
                
                return Builder<GetUserByAccountNumberResponse>.CreateNew()
                    .With(g => g.Name = user.Name)
                    .Build();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while retrieveing users.");
                throw new NotSuccessfulException("Failed to retrieve users.");
            }
        }

        public async Task<AccountCreationResponse> CreateAccount(AccountCreationRequest req)
        {
            try
            {
                var url = BuildUrl($"BankOneWebAPI/api/Account/CreateAccountQuick/2");

                _log.LogInformation($"CreateAccount Url: {url}");
                _log.LogInformation($"CreateAccount Request Body: {req}");

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

            
            _log.LogInformation($"GetBvnInfo Request Body: {jsonContent}");
            if (!bvnInfoResponse.isBvnValid)
            {
                throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Bvn is invalid");
            }

            //if (!bvnInfoResponse.bvnDetails.phoneNumber.Equals(phoneNo))
            //{
            //    throw new UssdMiddlewareException(ExceptionType.BAD_REQUEST,
            //        "Bvn does not belong to this phone number!");
            //}
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

            _log.LogInformation($"CheckAccountBalance Url: {url}");

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

                _log.LogInformation($"CreateAccount Url: {url}");

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
                    GLCode = _configuration["ApiOptions:Zikora:GLCode"],
                    NibssCode = _configuration["ApiOptions:Zikora:NibssCode"],
                    Fee = _configuration["ApiOptions:Zikora:FundTransferFee"],
                    token = authenticationToken,
                };

                var jsonContent = JsonConvert.SerializeObject(debitPayload);

                _log.LogInformation($"DebitCustomerAccount Url: {debitUrl}");
                _log.LogInformation($"CreateAccount Request Body: {jsonContent}");

                var debitResponseContent = await _httpService.Post<DebitCustomerAccountResponse>(debitUrl, headers, jsonContent);

                //if (debitResponseContent != null)
                if (debitResponseContent.IsSuccessful == true && debitResponseContent.ResponseCode == "00")
                {

                    var debitResult = new DebitCustomerAccountResponse
                    {
                            IsSuccessful = debitResponseContent.IsSuccessful,
                            ResponseMessage = debitResponseContent.ResponseMessage,
                            ResponseCode = debitResponseContent.ResponseCode,
                            Reference = debitResponseContent.Reference
                       
                    };

                    _log.LogInformation($"Debit result: {JsonConvert.SerializeObject(debitResult)}");

                    if (!debitResult.IsSuccessful)
                    {
                        _log.LogError($"Debit was not successful: {debitResult.ResponseMessage}");
                        throw new NotSuccessfulException($"Failed to debit customer account: {debitResult.ResponseMessage}");
                    }

                    _log.LogInformation($"Debit result: {JsonConvert.SerializeObject(debitResult)}");

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

        public async Task<CardResponse> CardRequest(CardRequestExtension request)
        {
            try
            {
                var authenticationToken = _apiOptions.Zikora.Token;
                var cardRequestUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Cards/RequestCard")}";


                var headers = BuildHeader();

                var cardRequest = new
                {
                    request.AccountNumber,
                    request.PhoneNumber,
                    request.NameOnCard,
                    request.BIN,
                    request.RequestType,
                    request.DeliveryOption,
                    request.Identifier,
                    token = authenticationToken,
                };

                var jsonContent = JsonConvert.SerializeObject(cardRequest);

                _log.LogInformation($"Card request Url: {cardRequestUrl}");
                _log.LogInformation($"Card Request Body: {cardRequest}");

                var cardResponseContent = await _httpService.Post<CardResponse>(cardRequestUrl, headers, jsonContent);

                if (cardResponseContent != null && cardResponseContent.IsSuccessful == true)
                {
                    var cardResult = new CardResponse
                    {
                        IsSuccessful = cardResponseContent.IsSuccessful,
                        ResponseMessage = cardResponseContent.ResponseMessage,
                    };

                    _log.LogInformation($"Card result: {JsonConvert.SerializeObject(cardResult)}");
                   
                    return cardResult;

                }
                else
                {
                    var errorMessage = cardResponseContent?.ResponseMessage;
                    _log.LogError($"Card request failed: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);
                    
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"An error occurred while making card request: {ex.Message}");
                throw new OperationFailedException($"Failed to make card request: {ex.Message}", ex);
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

                _log.LogInformation($"Status Query Url: {statusQueryUrl}");
                _log.LogInformation($"Card Request Body: {statusQueryJsonContent}");

               var statusQueryResponseContent = await _httpService.Post<RequeryResponse>(statusQueryUrl, headers, statusQueryJsonContent);

                if (statusQueryResponseContent.ResponseCode == "00" && statusQueryResponseContent.ResponseMessage == "Successful")
                {
                    var statusQueryResult = new RequeryResponse
                    {
                        IsSuccessful = statusQueryResponseContent.IsSuccessful,
                        ResponseMessage = statusQueryResponseContent.ResponseMessage,
                        ResponseCode = statusQueryResponseContent.ResponseCode,
                        Reference = statusQueryResponseContent.Reference,
                        Status = statusQueryResponseContent.Status,
                        
                    };

                    return statusQueryResult;
                }
                else
                {
                    _log.LogError($"Failed to query transaction status: {statusQueryResponseContent.ResponseMessage} ");
                    throw new NotSuccessfulException($"Failed to query transaction status: {statusQueryResponseContent.ResponseMessage}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Requery was unsuccessful: {ex.Message}");
                throw new OperationFailedException($"Requery Failed: {ex.Message}", ex);
            }
        }

        public async Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request)
        {
            try
            {
                var token = _apiOptions.Zikora.Token;
                var blockAccountUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Account/ActivatePND")}";


                var headers = BuildHeader();

                var blockAccount = new
                {
                    request.AccountNo,
                    authenticationCode = token,
                };

                var jsonContent = JsonConvert.SerializeObject(blockAccount);

                _log.LogInformation($"Block account Url: {blockAccountUrl}");
                _log.LogInformation($"Block account request Body: {jsonContent}");

                var response = await _httpService.Post<BlockAccountResponse>(blockAccountUrl, headers, jsonContent);

                if (response.RequestStatus == true && response.ResponseStatus == "Successful")
                {
                    var blockAccountResponse = new BlockAccountResponse
                    {
                        RequestStatus = response.RequestStatus,
                        ResponseDescription = response.ResponseDescription,
                        ResponseStatus = response.ResponseStatus,
                    };

                    _log.LogInformation($"Block account result: {JsonConvert.SerializeObject(blockAccountResponse)}");

                    return blockAccountResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseDescription ?? "An error occured while blocking account.";
                    _log.LogError($"Account blocking was unsuccessful: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Account blocking was unsuccessful: {ex.Message}");
                throw new OperationFailedException($"Failed to block account:{ex.Message}", ex);
            }
        }

        public async Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request)
        {
            try
            {
                var token = _apiOptions.Zikora.Token;
                var deactivatePostNoDebitUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Account/DeactivatePND")}";


                var headers = BuildHeader();

                var deactivatePostNoDebit = new
                {
                    request.AccountNo,
                    authenticationCode = token,
                };

                var jsonContent = JsonConvert.SerializeObject(deactivatePostNoDebit);

                _log.LogInformation($"DeactivatePND Url: {deactivatePostNoDebitUrl}");
                _log.LogInformation($"DeactivatePND Request Body: {jsonContent}");

                var response = await _httpService.Post<BlockAccountResponse>(deactivatePostNoDebitUrl, headers, jsonContent);

                if (response.RequestStatus = true && response.ResponseStatus == "Successful")
                {
                    var deactivatePostNoDebitResponse = new BlockAccountResponse
                    {
                        RequestStatus = response.RequestStatus,
                        ResponseDescription = response.ResponseDescription,
                        ResponseStatus = response.ResponseStatus,
                    };

                    _log.LogInformation($"PND deactivation result: {JsonConvert.SerializeObject(deactivatePostNoDebitResponse)}");

                    return deactivatePostNoDebitResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseDescription ?? "An error occured while deactivating PND.";
                    _log.LogError($"PND deactivation failed: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"PND deactivation failed: {ex.Message}");
                throw new OperationFailedException($"PND deactivation failed: {ex.Message}", ex);
            }
        }

        public async Task<BlockAccountResponse> VerifyPNDStatus(BlockAccountRequest request)
        {
            try
            {
                var token = _apiOptions.Zikora.Token;
                var verifyAccountPNDStatusUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Account/CheckPNDStatus")}";


                var headers = BuildHeader();

                var verifyAccountPNDStatus = new
                {
                    request.AccountNo,
                    authenticationCode = token,
                };

                var jsonContent = JsonConvert.SerializeObject(verifyAccountPNDStatus);

                _log.LogInformation($"VerifyPNDStatus Url: {verifyAccountPNDStatusUrl}");
                _log.LogInformation($"VerifyPNDStatus Request Body: {jsonContent}");

                var response = await _httpService.Post<BlockAccountResponse>(verifyAccountPNDStatusUrl, headers, jsonContent);

                if (response.RequestStatus == true && response.ResponseStatus == "Active")
                {
                    var verifyAccountPNDStatusResponse = new BlockAccountResponse
                    {
                        RequestStatus = response.RequestStatus,
                        ResponseDescription = response.ResponseDescription,
                        ResponseStatus = response.ResponseStatus,
                    };

                    _log.LogInformation($"Verify PND status result: {JsonConvert.SerializeObject(verifyAccountPNDStatusResponse)}");

                    return verifyAccountPNDStatusResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseDescription ?? "An error occured while verifying PND status.";
                    _log.LogError($"PND verification failed: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"PND verification failed: {ex.Message}");
                throw new OperationFailedException($"PND verification failed: {ex.Message}", ex);
            }
        }

        public async Task<GetCustomerCardResponse> GetCustomerCards(GetCustomerCardRequest request)
        {
            try
            {
                var authenticationToken = _apiOptions.Zikora.Token;
                var includeInactiveCards = true;
                var getCustomerCardsUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Cards/RetrieveCustomerCards")}";


                var headers = BuildHeader();

                var getCustomerCards = new
                {
                    request.AccountNo,
                    IncludeInactiveCards = includeInactiveCards,
                    token = authenticationToken,
                };

                var jsonContent = JsonConvert.SerializeObject(getCustomerCards);

                _log.LogInformation($"GetCustomerCards Url: {getCustomerCardsUrl}");
                _log.LogInformation($"GetCustomerCards Request Body: {jsonContent}");

                var response = await _httpService.Post<GetCustomerCardResponse>(getCustomerCardsUrl, headers, jsonContent);

                if (response.IsSuccessful)
                {
                    var getCustomerCardsResponse = new GetCustomerCardResponse
                    {
                        IsSuccessful = response.IsSuccessful,
                        ResponseDescription = response.ResponseDescription,
                        Cards = response.Cards.Select(card => new Card
                        {
                            AccountNumber = card.AccountNumber,
                            CardPAN = card.CardPAN,
                            LinkedDate = card.LinkedDate,
                            ExpiryDate = card.ExpiryDate,
                            SerialNo = card.SerialNo,
                            NameOnCard = card.NameOnCard,
                            Status = card.Status
                        }).ToArray()
                    };

                    _log.LogInformation($"Get customer card result: {JsonConvert.SerializeObject(response)}");

                    return response;

                }
                else
                {
                    var errorMessage = response?.ResponseDescription;
                    _log.LogError($"Get customer card result: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Get customer card result: {ex.Message}");
                throw new OperationFailedException($"{ex.Message}", ex);
            }
        }

        public async Task<FreezeCardResponse> FreezeCard(FreezeCardRequest request)
        {
            try
            {
                var token = _apiOptions.Zikora.Token;
                var freezeCardUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Cards/Freeze")}";

                var headers = BuildHeader();

                var freezeCard = new
                {
                    request.SerialNo,
                    request.Reference,
                    request.AccountNumber,
                    request.Reason,
                    token,
                };

                var jsonContent = JsonConvert.SerializeObject(freezeCard);

                _log.LogInformation($"FreezeCard Url: {freezeCardUrl}");
                _log.LogInformation($"FreezeCard Request Body: {jsonContent}");

                var response = await _httpService.Post<FreezeCardResponse>(freezeCardUrl, headers, jsonContent);

                if (response.IsSuccessful)
                {
                    var freezeCardResponse = new FreezeCardResponse
                    {
                        IsSuccessful = response.IsSuccessful,
                        ResponseCode = response.ResponseCode,
                        ResponseMessage = response.ResponseMessage,
                        TransactionReference = response.TransactionReference,
                    };

                    _log.LogInformation($"Freeze card result: {JsonConvert.SerializeObject(freezeCardResponse)}");

                    return freezeCardResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseMessage;
                    _log.LogError($"The attempt to freeze the card was unsuccessful: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"The attempt to freeze the card was unsuccessful: {ex.Message}");
                throw new OperationFailedException($"The attempt to freeze the card was unsuccessful:{ex.Message}", ex);
            }
        }

        public async Task<UnFreezeCardResponse> UnFreezeCard(UnFreezeCardRequest request)
        {
            try
            {
                var token = _apiOptions.Zikora.Token;
                var unFreezeCardUrl = $"{BuildUrl("/thirdpartyapiservice/apiservice/Cards/UnFreeze")}";


                var headers = BuildHeader();

                var UnfreezeCard = new
                {
                    request.SerialNo,
                    request.Reason,
                    request.AccountNumber,
                    token,
                };

                var jsonContent = JsonConvert.SerializeObject(UnfreezeCard);

                _log.LogInformation($"UnFreezeCard Url: {unFreezeCardUrl}");
                _log.LogInformation($"UnFreezeCard Request Body: {jsonContent}");

                var response = await _httpService.Post<UnFreezeCardResponse>(unFreezeCardUrl, headers, jsonContent);

                if (response.IsSuccessful)
                {
                    var unFreezeCardResponse = new UnFreezeCardResponse
                    {
                        IsSuccessful = response.IsSuccessful,
                        ResponseMessage = response.ResponseMessage,
                        Reference = response.Reference,
                    };

                    _log.LogInformation($"UnFreeze card result: {JsonConvert.SerializeObject(unFreezeCardResponse)}");

                    return unFreezeCardResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseMessage ?? "An error occured while unfreezing card.";
                    _log.LogError($"The attempt to unfreeze the card was unsuccessful.: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }
            }
            catch (Exception ex)
            {
                _log.LogError($"The attempt to unfreeze the card was unsuccessful: {ex.Message}");
                throw new OperationFailedException($"The attempt to unfreeze the card was unsuccessful:{ex.Message}", ex);
            }
        }

        public async Task<IntraBankTransferResponse> IntraBankTransfer(IntraBankTransferRequest model)
        {
            try
            {
                var authenticationKey = _apiOptions.Zikora.Token;

                var localFundsTransferUrl =
                    $"{_apiOptions.Zikora.BaseUrl}/thirdpartyapiservice/apiservice/CoreTransactions/LocalFundsTransfer";

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

                _log.LogInformation($"IntraBankTransfer Url: {localFundsTransferUrl}");
                _log.LogInformation($"IntraBankTransfer Request Body: {jsonContent}");

                var response = await _httpService.Post<IntraBankTransferResponse>(localFundsTransferUrl, headers, jsonContent);

               if(response.IsSuccessful && response.ResponseCode == "00")
                {
                    var intraBankTransferResponse = new IntraBankTransferResponse
                    {
                        IsSuccessful = response.IsSuccessful,
                        ResponseCode = response.ResponseCode,
                        ResponseMessage = response.ResponseMessage,
                        Reference = response.Reference,
                    };

                    _log.LogInformation($"Intrabank transfer result: {JsonConvert.SerializeObject(intraBankTransferResponse)}");

                    return intraBankTransferResponse;

                }
                else
                {
                    var errorMessage = response?.ResponseMessage;
                    _log.LogError($"Fund transfer failed: {errorMessage}");
                    throw new NotSuccessfulException(errorMessage);

                }

            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while performing intra-bank transfer.", ex);
                throw new NotSuccessfulException($"Failed to perform intra-bank transfer: {ex.Message}");
            }
        }

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




    

