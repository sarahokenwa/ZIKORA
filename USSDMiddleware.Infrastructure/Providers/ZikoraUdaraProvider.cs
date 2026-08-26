using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using USSDMiddleware.Core.Exceptions;
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
    public class ZikoraUdaraProvider
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


        //public Core.Enums.Providers ProviderType => Core.Enums.Providers.ZIKO RA;

        public ZikoraUdaraProvider(HttpClient httpClient,
        IOptions<UdaraOptions> options,
        UdaraMapper mapper,
        ILogger<ZikoraUdaraProvider> log)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _mapper = mapper;
            _log = log;
        }

        public async Task<AccountCreationResponse> CreateAccount(AccountCreationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // var customerId = await GetCustomerIdAsync(request, cancellationToken);
                //if (string.IsNullOrWhiteSpace(customerId))
                //{
                //    return new AccountCreationResponse(
                //        reference: _mapper.GetReference(request),
                //        customerId: null,
                //        accountNumber: null,
                //        message: "Unable to resolve customer ID.");
                //}
                var customerId = "";

                var reference = _mapper.GetReference(request);
                var payload = _mapper.MapToCreateAccountRequest(request, customerId, reference);
               // var token = await GetAccessTokenAsync(cancellationToken);

               
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/account/v1/createaccount");
               // httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Creating account. Reference: {Reference}", reference);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("Create account response: {StatusCode} {Body}", response.StatusCode, body);


                var udaraResponse = System.Text.Json.JsonSerializer.Deserialize<UdaraCreateAccountResponseModel>(body, JsonOptions);
                return _mapper.MapToAccountCreationResponse(
                    udaraResponse,
                    reference,
                    customerId,
                    request.AccountName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Error creating account");
                return new AccountCreationResponse(
                    reference: _mapper.GetReference(request),
                    customerId: null,
                    accountNumber: null,
                    message: "Account creation failed.");
            }
        }

        public async Task<BvnInfoResponse> GetBvnInfo(string bvn, string phoneNo, CancellationToken cancellationToken = default)
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
               // var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/Operations/v1/KYC/ValidateBVN");
              //  httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Validating BVN: {Bvn}", bvn);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("ValidateBVN response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraValidateBvnResponseModel>(body, JsonOptions);

                var result = _mapper.MapToBvnInfoResponse(udaraResponse);

                // Optional: keep the phone number check if you still need it
                // if (result.isBvnValid && 
                //     !string.Equals(result.bvnDetails?.phoneNumber, phoneNo, StringComparison.OrdinalIgnoreCase))
                // {
                //     return new BvnInfoResponse
                //     {
                //         RequestStatus = false,
                //         isBvnValid = false,
                //         ResponseMessage = "Bvn does not belong to this phone number!"
                //     };
                // }

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

        public async Task<BalanceEnquiryResponse> CheckAccountBalance(BalanceEnquiryRequest model, CancellationToken cancellationToken = default)
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

                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/getaccountbalancebyaccountnumber?AccountNumber={model.AccountNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Checking account balance. AccountNumber: {AccountNumber}", model.AccountNumber);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("Balance enquiry response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraBalanceEnquiryResponseModel>(body, JsonOptions);

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

        public async Task<List<GetAccountResponse>> GetAccountsByPhoneNumber(string phoneNumber, CancellationToken cancellationToken = default)
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

                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/GetAccountsByPhoneNumber?PhoneNumber={phoneNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Getting accounts by phone number: {PhoneNumber}", phoneNumber);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("GetAccountsByPhoneNumber response: {StatusCode} {Body}",
                    response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraGetAccountsByPhoneResponseModel>(body, JsonOptions);

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

        public async Task<GetUserByAccountNumberResponse> GetUserByAccountNumber(string accountNumber, CancellationToken cancellationToken = default)
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

                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                var url = $"/api/account/v1/getbyaccountnumber?AccountNumber={accountNumber}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("Getting user by account number: {AccountNumber}", accountNumber);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("GetUserByAccountNumber response: {StatusCode} {Body}",
                    response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraGetByAccountNumberResponseModel>(body, JsonOptions);

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

        public async Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request, CancellationToken cancellationToken = default)
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
                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/account/v1/deactivate");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Blocking account: {AccountNumber}", request.AccountNo);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("Block account response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraDeactivateAccountResponseModel>(body, JsonOptions);

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

        public async Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request, CancellationToken cancellationToken = default)
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
                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/api/account/v1/removepnd");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Deactivating PND for account: {AccountNumber}", request.AccountNo);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("DeactivatePND response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraDeactivateAccountResponseModel>(body, JsonOptions);

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

        public async Task<DebitCustomerAccountResponse> DebitCustomerAccount(
    DebitCustomerAccountRequest model,
    CancellationToken cancellationToken = default)
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
                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = model.RetrievalReference ?? Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/postings/v1/post");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Debiting account: {AccountNumber}, Amount: {Amount}",
                    model.AccountNumber, model.Amount);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("DebitCustomerAccount response: {StatusCode} {Body}",
                    response.StatusCode, body);

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

        public async Task<CardResponse> CardRequest(CardRequestExtension request, CancellationToken cancellationToken = default)
        {
            try
            {
              
                //  an endpoint that creates the card record is needed
                //It  returns a cardId, using AccountNumber, NameOnCard, BIN, etc.
                // -------------------------------------------------------
                var cardId = await CreateCardRecordAsync(request, cancellationToken);
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    return new CardResponse
                    {
                        IsSuccessful = false,
                        ResponseMessage = "Unable to create card record."
                    };
                }

                var payload = _mapper.MapToIssueCardRequest(cardId);
                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post, "/api/Card/v1/Interswitch/IssueCard");

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);
                httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

                _log.LogInformation("Issuing card. CardId: {CardId}", cardId);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

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

        public async Task<RequeryResponse> StatusQuery(ReQueryRequest model, CancellationToken cancellationToken = default)
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

                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];

                // transactionRef = value returned as InstrumentNumber / reference from the transfer
                // apiRequestRef  = the request-reference you sent when doing the original transfer
                var url = $"/api/Transfer/v1/TSQ?transactionRef={Uri.EscapeDataString(model.RetrievalReference)}";

                // If you also stored the original request-reference, append it:
                // url += $"&apiRequestRef={Uri.EscapeDataString(originalRequestRef)}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("StatusQuery. RetrievalReference: {Ref}", model.RetrievalReference);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("StatusQuery response: {StatusCode} {Body}", response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraTsqResponseModel>(body, JsonOptions);

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

        public async Task<GetCustomerCardResponse> GetCustomerCards(GetCustomerCardRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.AccountNo))
                {
                    return new GetCustomerCardResponse
                    {
                        IsSuccessful = false,
                        ResponseDescription = "Account number is required.",
                        Cards = null
                    };
                }

                // 1. Resolve customerId from account number
                var customerId = await GetCustomerIdByAccountNumberAsync(request.AccountNo, cancellationToken);
                if (string.IsNullOrWhiteSpace(customerId))
                {
                    return new GetCustomerCardResponse
                    {
                        IsSuccessful = false,
                        ResponseDescription = "Unable to resolve customer for this account.",
                        Cards = null
                    };
                }

                // 2. Get cards by customerId
                var token = await GetAccessTokenAsync(cancellationToken);
                var reference = Guid.NewGuid().ToString("N")[..20];
                var url = $"/api/Card/v1/Interswitch/GetCardAccountByCustomerId?customerId={customerId}";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

                _log.LogInformation("GetCustomerCards. AccountNo: {AccountNo}, CustomerId: {CustomerId}",
                    request.AccountNo, customerId);

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                _log.LogInformation("GetCustomerCards response: {StatusCode} {Body}",
                    response.StatusCode, body);

                var udaraResponse = System.Text.Json.JsonSerializer
                    .Deserialize<UdaraGetCardAccountResponseModel>(body, JsonOptions);

                var result = _mapper.MapToGetCustomerCardResponse(udaraResponse);

                // Optional: filter by account number if you only want cards for that account
                if (result.IsSuccessful && result.Cards != null && !string.IsNullOrWhiteSpace(request.AccountNo))
                {
                    result.Cards = result.Cards
                        .Where(c => string.Equals(c.AccountNumber, request.AccountNo, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }

                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Get customer cards failed");
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Uses the existing getbyaccountnumber endpoint to obtain customerID.
        /// </summary>
      

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
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

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError("Token request failed: {Status} {Body}", response.StatusCode, content);
                throw new InvalidOperationException("Unable to obtain access token.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            _cachedToken = root.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("accessToken not found in response.");

            var validitySec = root.TryGetProperty("tokenValiditySec", out var validity)
                ? validity.GetInt64()
                : 3600;

            _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(validitySec);

            return _cachedToken;
        }

        /// <summary>
        /// This should return the cardId required by IssueCard.
        /// </summary>
        private Task<string?> CreateCardRecordAsync(CardRequestExtension request, CancellationToken cancellationToken)
        {
            _log.LogWarning(
                "CreateCardRecordAsync is not implemented. " +
                "IssueCard only accepts cardId. Need the endpoint that creates the card first.");

            return Task.FromResult<string?>(null);
        }

        private async Task<string?> GetCustomerIdByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken)
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            var reference = Guid.NewGuid().ToString("N")[..20];
            var url = $"/api/account/v1/getbyaccountnumber?AccountNumber={accountNumber}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Headers.TryAddWithoutValidation("request-reference", reference);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

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
    }
}
