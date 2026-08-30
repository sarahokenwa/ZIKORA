using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models.V2;
using USSDMiddleware.Core.Models.V2.Mapper;
using USSDMiddleware.Core.Models.V2.Response;

namespace USSDMiddleware.Infrastructure.Providers
{
    public class ZikoraUdaraProvider : IUssdProvider
    {
        private readonly HttpClient _httpClient;
        private readonly UdaraOptions _options;
        private readonly UdaraMapper _mapper;
        private readonly ILogger<ZikoraUdaraProvider> _log;

        private string? _cachedToken;
        private DateTimeOffset _tokenExpiresAt;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public Core.Enums.Providers ProviderType => Core.Enums.Providers.ZIKORA;


        public ZikoraUdaraProvider(IHttpClientFactory httpClientFactory,
        IOptions<UdaraOptions> options,
        UdaraMapper mapper,
        ILogger<ZikoraUdaraProvider> log)
        {
            _httpClient = httpClientFactory.CreateClient("Udara");
            _options = options.Value;
            _mapper = mapper;
            _log = log;
        }

        public async Task<AccountCreationResponse> CreateAccount(AccountCreationRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req?.BVN))
                {
                    return new AccountCreationResponse(reference: _mapper.GetReference(req), customerId: null, accountNumber: null, message: "BVN is required.");
                }

                var bvnInfo = await GetBvnInfo(req.BVN, req.PhoneNo);

                if (!bvnInfo.RequestStatus || !bvnInfo.isBvnValid || bvnInfo.bvnDetails is null)
                {
                    return new AccountCreationResponse(
                        reference: _mapper.GetReference(req),
                        customerId: null,
                        accountNumber: null,
                        message: bvnInfo.ResponseMessage ?? "BVN validation failed.");
                }

                var reference = _mapper.GetReference(req);
                var payload = _mapper.MapToCreateCustomerAccountRequest(req, bvnInfo.bvnDetails);
                var fullName = payload.AccountName;

                var token = await GetAccessTokenAsync();

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/account/v1/createcustomeraccount");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Creating customer + account. Reference: {Reference}, BVN: {Bvn}", reference, MaskBvn(req.BVN));

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("CreateCustomerAccount response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraCreateAccountResponseModel>(body, JsonOptions);

                return _mapper.MapToAccountCreationResponse(udaraResponse, reference, fullName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error creating account");
                return new AccountCreationResponse(
                    reference: _mapper.GetReference(req),
                    customerId: null,
                    accountNumber: null,
                    message: "Account creation failed.");
            }
        }

        public async Task<BvnInfoResponse> GetBvnInfo(string bvn, string phoneNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bvn))
                {
                    return new BvnInfoResponse
                    {
                        RequestStatus = false,
                        isBvnValid = false,
                        ResponseMessage = "BVN is required."
                    };
                }

                var payload = _mapper.MapToValidateBvnRequest(bvn);
                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/Operations/v1/KYC/ValidateBVN");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Validating BVN: {Bvn}", MaskBvn(bvn));

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("ValidateBVN response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraValidateBvnResponseModel>(body, JsonOptions);

                var result = _mapper.MapToBvnInfoResponse(udaraResponse);

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error validating BVN");
                return new BvnInfoResponse
                {
                    RequestStatus = false,
                    isBvnValid = false,
                    ResponseMessage = "Unable to validate your bvn at the moment, try again later!"
                };
            }
        }

        public async Task<BalanceEnquiryResponse> CheckAccountBalance(BalanceEnquiryRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.AccountNumber))
                {
                    return new BalanceEnquiryResponse
                    {
                        Message = "Account number is required."
                    };
                }

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/getaccountbalancebyaccountnumber?AccountNumber={model.AccountNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Checking account balance. AccountNumber: {AccountNumber}", model.AccountNumber);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("Balance enquiry response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraBalanceEnquiryResponseModel>(body, JsonOptions);

                return _mapper.MapToBalanceEnquiryResponse(udaraResponse);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error retrieving account balance");
                return new BalanceEnquiryResponse
                {
                    Message = "Failed to retrieve account balance."
                };
            }
        }

        public async Task<List<GetAccountResponse>> GetAccountsByPhoneNumber(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return new List<GetAccountResponse>
                    {
                        new GetAccountResponse { Message = "Phone number is required." }
                    };
                }

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/GetAccountsByPhoneNumber?PhoneNumber={phoneNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Getting accounts by phone number: {PhoneNumber}", phoneNumber);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("GetAccountsByPhoneNumber response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraGetAccountsByPhoneResponseModel>(body, JsonOptions);

                return _mapper.MapToGetAccountResponseList(udaraResponse, phoneNumber);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error retrieving accounts by phone number");
                return new List<GetAccountResponse>
                {
                    new GetAccountResponse
                    {
                        Message = $"No customer found with phone number {phoneNumber}."
                    }
                };
            }
        }

        public async Task<GetUserByAccountNumberResponse> GetUserByAccountNumber(string accountNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountNumber))
                {
                    return new GetUserByAccountNumberResponse
                    {
                        Name = null,
                        ErrorMessage = "Account number is required."
                    };
                }

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/getbyaccountnumber?AccountNumber={accountNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Getting user by account number: {AccountNumber}", accountNumber);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("GetUserByAccountNumber response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraGetByAccountNumberResponseModel>(body, JsonOptions);

                return _mapper.MapToGetUserByAccountNumberResponse(udaraResponse);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error retrieving user by account number");
                return new GetUserByAccountNumberResponse
                {
                    Name = null,
                    ErrorMessage = "User not found."
                };
            }
        }

        public async Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                var payload = _mapper.MapToDeactivateAccountRequest(request);
                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/account/v1/deactivate");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Blocking account: {AccountNumber}", request.AccountNo);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("Block account response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraDeactivateAccountResponseModel>(body, JsonOptions);

                return _mapper.MapToBlockAccountResponse(udaraResponse);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error blocking account");
                return new BlockAccountResponse
                {
                    RequestStatus = false,
                    ResponseDescription = "Failed to block account.",
                    ResponseStatus = "Failed"
                };
            }
        }

        public async Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                var payload = _mapper.MapToRemovePndRequest(request);
                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/account/v1/removepnd");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Deactivating PND for account: {AccountNumber}", request.AccountNo);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("DeactivatePND response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraDeactivateAccountResponseModel>(body, JsonOptions);

                var result = _mapper.MapToDeactivatePndResponse(udaraResponse);

                if (!result.RequestStatus || result.ResponseStatus != "Successful")
                {
                    var errorMessage = result.ResponseDescription ?? "An error occurred while deactivating PND.";
                    _log.LogError("PND deactivation failed: {Error}", errorMessage);
                    throw new NotSuccessfulException(errorMessage);
                }

                return result;
            }
            catch (NotSuccessfulException)
            {
                throw; 
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "PND deactivation failed");
                throw new OperationFailedException($"PND deactivation failed: {ex.Message}", ex);
            }
        }

        public async Task<DebitCustomerAccountResponse> DebitCustomerAccount(DebitCustomerAccountRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.AccountNumber) ||
                    string.IsNullOrWhiteSpace(model?.GLCode))
                {
                    return new DebitCustomerAccountResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Account number and GL code are required."
                    };
                }

                var payload = _mapper.MapToDebitPostingRequest(model);
                var token = await GetAccessTokenAsync();
                var reference = model.RetrievalReference ?? Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/postings/v1/post");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Debiting account: {AccountNumber}, Amount: {Amount}", model.AccountNumber, model.Amount);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("DebitCustomerAccount response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraPostingResponseModel>(body, JsonOptions);

                var result = _mapper.MapToDebitCustomerAccountResponse(udaraResponse, reference);

                if (!result.IsSuccessful)
                {
                    _log.LogError("Debit was not successful: {Message}", result.ResponseMessage);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error debiting customer account");
                return new DebitCustomerAccountResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Failed to debit customer account."
                };
            }
        }

        public async Task<CardResponse> CardRequest(CardRequestExtension request)
        {
            try
            {
              
                //  an endpoint that creates the card record is needed
                //It  returns a cardId, using AccountNumber, NameOnCard, BIN, etc.
                // -------------------------------------------------------
                var cardId = await CreateCardRecordAsync(request);
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    return new CardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Unable to create card record."
                    };
                }

                var payload = _mapper.MapToIssueCardRequest(cardId);
                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post, "/api/Card/v1/Interswitch/IssueCard");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Issuing card. CardId: {CardId}", cardId);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("IssueCard response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraIssueCardResponseModel>(body, JsonOptions);

                return _mapper.MapToCardResponse(udaraResponse);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error making card request");
                return new CardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = $"An error occurred while making card request: {ex.Message}"
                };
            }
        }

        public async Task<RequeryResponse> StatusQuery(ReQueryRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.RetrievalReference))
                {
                    return new RequeryResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Retrieval reference is required.",
                        Status = "Failed"
                    };
                }

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/Transfer/v1/TSQ?transactionRef={Uri.EscapeDataString(model.RetrievalReference)}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("StatusQuery. RetrievalReference: {Ref}", model.RetrievalReference);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("StatusQuery response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraTsqResponseModel>(body, JsonOptions);

                return _mapper.MapToRequeryResponse(udaraResponse);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Requery was unsuccessful");
                return new RequeryResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Requery Failed",
                    Status = "Failed"
                };
            }
        }

        public async Task<GetCustomerCardResponse> GetCustomerCards(GetCustomerCardRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNo) &&
                    string.IsNullOrWhiteSpace(request?.CustomerId))
                {
                    return new GetCustomerCardResponse
                    {
                        IsSuccessful = false,
                        ResponseDescription = "Account number or customer ID is required.",
                        Cards = null
                    };
                }

                var customerId = request.CustomerId;
                if (string.IsNullOrWhiteSpace(customerId))
                {
                    customerId = await GetCustomerIdByAccountNumberAsync(request.AccountNo);
                }

                if (string.IsNullOrWhiteSpace(customerId))
                {
                    return new GetCustomerCardResponse
                    {
                        IsSuccessful = false,
                        ResponseDescription = "Unable to resolve customer for this account.",
                        Cards = null
                    };
                }

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];
                var url = $"/api/Card/v1/Interswitch/GetCardAccountByCustomerId?customerId={customerId}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("GetCustomerCards. AccountNo: {AccountNo}, CustomerId: {CustomerId}", request.AccountNo, customerId);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("GetCustomerCards response: {StatusCode} {Body}",
                    response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraGetCardAccountResponseModel>(body, JsonOptions);

                var result = _mapper.MapToGetCustomerCardResponse(udaraResponse);

                // Optional: only cards for this account
                if (result.IsSuccessful && result.Cards != null && !string.IsNullOrWhiteSpace(request.AccountNo))
                {
                    result.Cards = result.Cards.Where(c => string.Equals(c.AccountNumber, request.AccountNo, StringComparison.OrdinalIgnoreCase)).ToArray();
                }

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Get customer cards failed");
                throw new OperationFailedException(ex.Message, ex);
            }
        }
        public async Task<FreezeCardResponse> FreezeCard(FreezeCardRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNumber))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Account number is required."
                    };
                }

                var cardId = await ResolveCardIdAsync(request.AccountNumber, request.SerialNo);
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    return new FreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Unable to resolve card for this account."
                    };
                }

                var payload = _mapper.MapToUpdateCardStatusRequest(
                    cardId,
                    status: 1, // Block = Freeze
                    request.Reason);

                var token = await GetAccessTokenAsync();
                var reference = request.Reference ?? Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/Card/v1/Interswitch/UpdateCardStatus");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Freezing card. CardId: {CardId}, Account: {Account}", cardId, request.AccountNumber);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("FreezeCard response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraUpdateCardStatusResponseModel>(body, JsonOptions);

                return _mapper.MapToFreezeCardResponse(udaraResponse, reference);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Freeze card failed");
                return new FreezeCardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = $"An error occurred while freezing the card: {ex.Message}"
                };
            }
        }

        public async Task<UnFreezeCardResponse> UnFreezeCard(UnFreezeCardRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNumber))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Account number is required."
                    };
                }

                var cardId = await ResolveCardIdAsync(request.AccountNumber, request.SerialNo);
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    return new UnFreezeCardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Unable to resolve card for this account."
                    };
                }

                var payload = _mapper.MapToUpdateCardStatusRequest(
                    cardId,
                    status: 2, // Unblock = Unfreeze
                    request.Reason);

                var token = await GetAccessTokenAsync();
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/Card/v1/Interswitch/UpdateCardStatus");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Unfreezing card. CardId: {CardId}, Account: {Account}", cardId, request.AccountNumber);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("UnFreezeCard response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraUpdateCardStatusResponseModel>(body, JsonOptions);

                return _mapper.MapToUnFreezeCardResponse(udaraResponse, reference);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Unfreeze card failed");
                return new UnFreezeCardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = $"An error occurred while unfreezing the card: {ex.Message}"
                };
            }
        }

        public async Task<string> GetProviderId(IProviderManager providerManager)
        {
            var provider = await providerManager.GetProviderByName(Core.Enums.Providers.ZIKORA.ToString());
            return provider.Id;
        }

        public async Task<IntraBankTransferResponse> IntraBankTransfer(IntraBankTransferRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.FromAccountNumber) ||
                    string.IsNullOrWhiteSpace(model?.ToAccountNumber))
                {
                    return new IntraBankTransferResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "From and To account numbers are required."
                    };
                }

                var payload = _mapper.MapToLocalFundTransferRequest(model);
                var token = await GetAccessTokenAsync();
                var reference = model.RetrievalReference ?? Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/transfer/v1/localfundtransfer");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("IntraBankTransfer. From: {From}, To: {To}, Amount: {Amount}", model.FromAccountNumber, model.ToAccountNumber, model.Amount);

                using var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                _log.LogInformation("IntraBankTransfer response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraLocalFundTransferResponseModel>(body, JsonOptions);

                return _mapper.MapToIntraBankTransferResponse(udaraResponse, reference);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error performing intra-bank transfer");
                return new IntraBankTransferResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Intrabank transfer failed."
                };
            }
        }
        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) &&
                DateTimeOffset.UtcNow < _tokenExpiresAt.AddMinutes(-1))
            {
                return _cachedToken;
            }

            var tokenRequest = new
            {
                clientId = _options.ClientId,
                clientSecret = _options.ClientSecret
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
            request.Content = JsonContent.Create(tokenRequest);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("Token request failed: {Status} {Body}", response.StatusCode, content);
                throw new InvalidOperationException("Unable to obtain access token.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            _cachedToken = root.GetProperty("accessToken").GetString() ?? throw new InvalidOperationException("accessToken not found in response.");

            var validitySec = root.TryGetProperty("tokenValiditySec", out var validity) ? validity.GetInt64() : 3600;

            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(validitySec);

            return _cachedToken;
        }

        /// <summary>
        /// This should return the cardId required by IssueCard.
        /// </summary>
        private Task<string?> CreateCardRecordAsync(CardRequestExtension request)
        {
            _log.LogWarning(
                "CreateCardRecordAsync is not implemented. " +
                "IssueCard only accepts cardId. Need the endpoint that creates the card first.");

            return Task.FromResult<string?>(null);
        }

        private async Task<string?> GetCustomerIdByAccountNumberAsync(string accountNumber)
        {
            var token = await GetAccessTokenAsync();
            var reference = Guid.NewGuid().ToString("N")[..20];
            var url = $"/api/account/v1/getbyaccountnumber?AccountNumber={accountNumber}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

            using var response = await _httpClient.SendAsync(httpRequest);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("status", out var statusProp) || !statusProp.GetBoolean())
                return null;

            if (!root.TryGetProperty("data", out var data))
                return null;

            if (data.TryGetProperty("customerID", out var customerIdProp))
                return customerIdProp.GetString();

            if (data.TryGetProperty("customerId", out var customerIdProp2))
                return customerIdProp2.GetString();

            return null;
        }

        private async Task<string?> ResolveCardIdAsync(string accountNumber, string? serialNo)
        {
            var customerId = await GetCustomerIdByAccountNumberAsync(accountNumber);
            if (string.IsNullOrWhiteSpace(customerId))
                return null;

            var token = await GetAccessTokenAsync();
            var reference = Guid.NewGuid().ToString("N")[..20];
            var url = $"/api/Card/v1/Interswitch/GetCardAccountByCustomerId?customerId={customerId}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

            using var response = await _httpClient.SendAsync(httpRequest);
            var body = await response.Content.ReadAsStringAsync();

            var udaraResponse = System.Text.Json.JsonSerializer
                .Deserialize<UdaraGetCardAccountResponseModel>(body, JsonOptions);

            if (udaraResponse?.Data?.Data is null || udaraResponse.Data.Data.Count == 0)
                return null;

            // Prefer match by serial number if provided
            if (!string.IsNullOrWhiteSpace(serialNo))
            {
                var bySerial = udaraResponse.Data.Data
                    .FirstOrDefault(x => string.Equals(x.Card?.SerialNumber, serialNo, StringComparison.OrdinalIgnoreCase));

                if (bySerial != null && !string.IsNullOrWhiteSpace(bySerial.CardId))
                    return bySerial.CardId;
            }

            // Fallback: match by account number
            var byAccount = udaraResponse.Data.Data
                .FirstOrDefault(x => string.Equals(x.AccountNumber, accountNumber, StringComparison.OrdinalIgnoreCase));

            return byAccount?.CardId
                ?? udaraResponse.Data.Data.FirstOrDefault()?.CardId;
        }

        private static string MaskBvn(string? bvn)
        {
            if (string.IsNullOrWhiteSpace(bvn))
                return "****";

            return bvn.Length <= 4
                ? "****"
                : new string('*', bvn.Length - 4) + bvn[^4..];
        }

        /////////////////This isn't used by Zikora, but is here for reference./////////////////////
        public Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
            {
                return Task.FromResult(new PhoneValidationResponse(
                    status: false,
                    canRegister: false,
                    message: "Phone number is required."));
            }

            return Task.FromResult(new PhoneValidationResponse(
                status: true,
                canRegister: true,
                message: "Successful"));
        }

        public Task<GetUserByPhoneNumberResponse> GetUserByPhoneNumber(string phoneNumber)
        {
            return Task.FromResult(new GetUserByPhoneNumberResponse
            {
                PhoneNumber = phoneNumber,
                DateOfBirth = null,
                BankVerificationNumber = null,
                Email = null
            });
        }

        public Task<BlockAccountResponse> VerifyPNDStatus(BlockAccountRequest request)
        {
            return Task.FromResult(new BlockAccountResponse
            {
                RequestStatus = false,
                ResponseDescription = "PND status verification is not supported for UDARA provider.",
                ResponseStatus = "Failed"
            });
        }

        public Task<SMSResponse> SendSMS(SMSRequest request)
        {
            return Task.FromResult(new SMSResponse
            {
                IsSuccess = false,
                ResponseMessage = "SMS is not handled by the UDARA provider."
            });
        }


    }
}
