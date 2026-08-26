using Microsoft.Extensions.Options;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Providers.Zikora;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models.V2.Request;
using USSDMiddleware.Core.Models.V2.Response;

namespace USSDMiddleware.Core.Models.V2.Mapper
{
    public class UdaraMapper
    {
                    
        private readonly UdaraOptions _options;
        public UdaraMapper(IOptions<UdaraOptions> options) 
        {
            _options = options.Value;
        }


        public UdaraCreateAccountRequestModel MapToCreateAccountRequest(AccountCreationRequest source, string customerId, string reference)
        {
            return new UdaraCreateAccountRequestModel
            {
                CustomerID = customerId,
                AccountName = string.IsNullOrWhiteSpace(source.AccountName)? $"{source.FirstName} {source.LastName}".Trim()  : source.AccountName,
                ReferenceNumber = reference,
                ProductCode = string.IsNullOrWhiteSpace(source.ProductCode) ? _options.DefaultProductCode : source.ProductCode,

                AccountOfficerStaffID = string.IsNullOrWhiteSpace(source.AccountOfficerCode)
                    ? _options.DefaultAccountOfficerStaffId
                    : source.AccountOfficerCode,

                BranchCode = _options.DefaultBranchCode,
                AccountTierLevel = ParseAccountTier(source.AccountTier),
                AccessLevel = _options.DefaultAccessLevel,
                AccountType = _options.DefaultAccountType,
                AccountStatus = _options.DefaultAccountStatus,
                StatementDeliveryMode = _options.DefaultStatementDeliveryMode,
                StatementDeliveryFrequency = _options.DefaultStatementDeliveryFrequency,
                MinimumBalanceRequired = _options.DefaultMinimumBalanceRequired,
                CategoryOfAccount = _options.DefaultCategoryOfAccount,
                SectorCode = _options.DefaultSectorCode,

                EnableEmailNotification = true,
                EnableSMSNotification = true,
                GroupConnectionID = string.Empty,
                IsMinor = false
            };
        }

        public AccountCreationResponse MapToAccountCreationResponse(UdaraCreateAccountResponseModel? source, string reference, string? customerId = null, string? fullName = null)
        {
            if (source is null)
            {
                return new AccountCreationResponse(
                    reference: reference,
                    customerId: customerId,
                    accountNumber: null,
                    fullName: fullName,
                    message: "Invalid response from provider.");
            }

            if (!source.Status)
            {
                return new AccountCreationResponse(
                    reference: reference,
                    customerId: customerId,
                    accountNumber: null,
                    fullName: fullName,
                    message: string.IsNullOrWhiteSpace(source.Message) ? "Account creation failed."  : source.Message);
            }

            return new AccountCreationResponse(
                reference: reference,
                customerId: customerId,
                accountNumber: source.Data?.AccountNumber,
                fullName: fullName,
                message: string.IsNullOrWhiteSpace(source.Message) ? "Account created successfully": source.Message);
        }

        private int ParseAccountTier(string? tier)
        {
            if (int.TryParse(tier, out var value) && value is >= 1 and <= 3)
                return value;

            return _options.DefaultAccountTier;
        }

        public string GetReference(AccountCreationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.AccountOpeningTrackingRef))
                return request.AccountOpeningTrackingRef;

            if (!string.IsNullOrWhiteSpace(request.TransactionTrackingRef))
                return request.TransactionTrackingRef;

            return Guid.NewGuid().ToString("N")[..20];
        }

        public UdaraValidateBvnRequestModel MapToValidateBvnRequest(string bvn)
        {
            return new UdaraValidateBvnRequestModel
            {
                Bvn = bvn,
                IncludeData = 1
            };
        }

        public BvnInfoResponse MapToBvnInfoResponse(UdaraValidateBvnResponseModel? source)
        {
            if (source is null || !source.Status || source.Data?.Result is null)
            {
                return new BvnInfoResponse
                {
                    RequestStatus = false,
                    isBvnValid = false,
                    ResponseMessage = source?.Message ?? "BVN validation failed."
                };
            }

            var result = source.Data.Result;

            return new BvnInfoResponse
            {
                RequestStatus = true,
                isBvnValid = true,
                ResponseMessage = source.Message ?? "Verification Successful",
                bvnDetails = new BvnDetails
                {
                    BVN = result.Bvn,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    OtherNames = result.MiddleName,
                    DOB = result.DateOfBirth,
                    Email = result.Email,
                    phoneNumber = result.PhoneNumber1
                }
            };
        }

        public BalanceEnquiryResponse MapToBalanceEnquiryResponse(UdaraBalanceEnquiryResponseModel? source)
        {
            if (source is null || !source.Status || source.Data is null)
            {
                return new BalanceEnquiryResponse
                {
                    Message = source?.Message ?? "Failed to retrieve account balance."
                };
            }

            return new BalanceEnquiryResponse
            {
                AvailableBalance = source.Data.AvailableBalance.ToString("0.00"),
                LedgerBalance = source.Data.LedgerBalance.ToString("0.00"),
                WithdrawableBalance = source.Data.WithdrawableBalance.ToString("0.00"),
                Message = source.Message
            };
        }

        public List<GetAccountResponse> MapToGetAccountResponseList(UdaraGetAccountsByPhoneResponseModel? source, string phoneNumber)
        {
            if (source is null || !source.Status || source.Data?.Data is null || source.Data.Data.Count == 0)
            {
                return new List<GetAccountResponse>
        {
            new GetAccountResponse
            {
                Message = source?.Message ?? $"No accounts found for phone number {phoneNumber}."
            }
        };
            }

            return source.Data.Data.Select(account => new GetAccountResponse
            {
                AccountNumber = account.AccountNumber,
                AccountType = account.AccountType,
                AccountStatus = account.AccountStatus,
                AccessLevel = null, 
                Message = "Account retrieved successfully."
            }).ToList();
        }

        public GetUserByAccountNumberResponse MapToGetUserByAccountNumberResponse(UdaraGetByAccountNumberResponseModel? source)
        {
            if (source is null || !source.Status || source.Data is null)
            {
                return new GetUserByAccountNumberResponse
                {
                    Name = null,
                    ErrorMessage = source?.Message ?? "User not found."
                };
            }

            var name = !string.IsNullOrWhiteSpace(source.Data.AccountName)
                ? source.Data.AccountName
                : source.Data.CustomerName;

            return new GetUserByAccountNumberResponse
            {
                Name = name,
                ErrorMessage = null
            };
        }

        public UdaraDeactivateAccountRequestModel MapToDeactivateAccountRequest(BlockAccountRequest source)
        {
            return new UdaraDeactivateAccountRequestModel
            {
                AccountNumber = source.AccountNo
            };
        }

        public BlockAccountResponse MapToBlockAccountResponse(UdaraDeactivateAccountResponseModel? source)
        {
            if (source is null)
            {
                return new BlockAccountResponse
                {
                    RequestStatus = false,
                    ResponseDescription = "Failed to block account.",
                    ResponseStatus = "Failed"
                };
            }

            return new BlockAccountResponse
            {
                RequestStatus = source.Status,
                ResponseDescription = string.IsNullOrWhiteSpace(source.Message)
                    ? (source.Status ? "Account blocked successfully." : "Failed to block account.")
                    : source.Message,
                ResponseStatus = source.Status ? "Successful" : "Failed"
            };
        }

        public UdaraRemovePndRequestModel MapToRemovePndRequest(BlockAccountRequest source)
        {
            return new UdaraRemovePndRequestModel
            {
                AccountNumber = source.AccountNo
            };
        }

        public BlockAccountResponse MapToDeactivatePndResponse(UdaraDeactivateAccountResponseModel? source)
        {
            if (source is null)
            {
                return new BlockAccountResponse
                {
                    RequestStatus = false,
                    ResponseDescription = "An error occurred while deactivating PND.",
                    ResponseStatus = "Failed"
                };
            }

            return new BlockAccountResponse
            {
                RequestStatus = source.Status,
                ResponseDescription = string.IsNullOrWhiteSpace(source.Message)
                    ? (source.Status ? "PND deactivated successfully." : "An error occurred while deactivating PND.")
                    : source.Message,
                ResponseStatus = source.Status ? "Successful" : "Failed"
            };
        }

        public UdaraLocalFundTransferRequestModel MapToLocalFundTransferRequest(IntraBankTransferRequest source)
        {
            var amountInKobo = (long)(source.Amount * 100);
            var feeInKobo = source.Fee.HasValue ? (long)(source.Fee.Value * 100) : 0;

            return new UdaraLocalFundTransferRequestModel
            {
                DebitAccount = source.FromAccountNumber,
                CreditAccount = source.ToAccountNumber,
                Amount = amountInKobo,
                FeeCharge = feeInKobo,
                FeeIncomeGL = feeInKobo > 0 ? _options.DefaultFeeIncomeGL.ToString() : null,
                InstrumentNumber = source.RetrievalReference,
                Narration = source.Narration?.Length > 40
                    ? source.Narration[..40]
                    : source.Narration ?? "Transfer",
                PostingsTransactionType = 3 
            };
        }

        public IntraBankTransferResponse MapToIntraBankTransferResponse(UdaraLocalFundTransferResponseModel? source, string? fallbackReference = null)
        {
            if (source is null)
            {
                return new IntraBankTransferResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Intrabank transfer failed.",
                    ResponseCode = null,
                    Reference = fallbackReference
                };
            }

            return new IntraBankTransferResponse
            {
                IsSuccessful = source.Status,
                ResponseMessage = string.IsNullOrWhiteSpace(source.Message)
                    ? (source.Status ? "Transfer successful." : "Intrabank transfer failed.")
                    : source.Message,
                ResponseCode = source.Data?.StatusCode,
                Reference = source.Data?.ReferenceNumber ?? source.Data?.InstrumentNumber ?? fallbackReference
            };
        }
        public UdaraPostingRequestModel MapToDebitPostingRequest(DebitCustomerAccountRequest source)
        {
            var amountInKobo = (long)(source.Amount * 100);
            var feeInKobo = (long)(source.Fee * 100);
            var totalInKobo = amountInKobo + feeInKobo;

            var narration = string.IsNullOrWhiteSpace(source.Narration)
                ? "Debit"
                : source.Narration.Length > 40 ? source.Narration[..40] : source.Narration;

            var instrument = source.RetrievalReference;

            var entries = new List<UdaraPostingEntry>
    {
        // Debit the customer
        new UdaraPostingEntry
        {
            AccountNumber = source.AccountNumber,
            Amount = totalInKobo,
            RecordType = 1, // Debit
            Narration = narration,
            InstrumentNumber = instrument
        },
        // Credit the GL
        new UdaraPostingEntry
        {
            AccountNumber = source.GLCode,
            Amount = totalInKobo,
            RecordType = 2, // Credit
            Narration = narration,
            InstrumentNumber = instrument
        }
    };

            return new UdaraPostingRequestModel
            {
                PostingEntryRequest = entries,
                PostingDataRequest = new UdaraPostingData()
            };
        }

        public DebitCustomerAccountResponse MapToDebitCustomerAccountResponse(UdaraPostingResponseModel? source, string? fallbackReference = null)
        {
            if (source is null)
            {
                return new DebitCustomerAccountResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Failed to debit customer account."
                };
            }

            var isSuccess = source.Status &&
                            (string.IsNullOrEmpty(source.Data?.StatusCode) || source.Data.StatusCode == "00");

            return new DebitCustomerAccountResponse
            {
                IsSuccessful = isSuccess,
                ResponseMessage = string.IsNullOrWhiteSpace(source.Message)
                    ? (isSuccess ? "Debit successful." : "Failed to debit customer account.")
                    : source.Message,
                ResponseCode = source.Data?.StatusCode,
                Reference = source.Data?.ReferenceNumber ?? source.Data?.InstrumentNumber ?? fallbackReference
            };
        }

        public UdaraIssueCardRequestModel MapToIssueCardRequest(string cardId)
        {
            return new UdaraIssueCardRequestModel
            {
                CardId = cardId
            };
        }

        public CardResponse MapToCardResponse(UdaraIssueCardResponseModel? source)
        {
            if (source is null)
            {
                return new CardResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Card request failed."
                };
            }

            return new CardResponse
            {
                IsSuccessful = source.Status,
                ResponseMessage = string.IsNullOrWhiteSpace(source.Message)
                    ? (source.Status ? "Operation Successful" : "Card request failed.")
                    : source.Message
            };
        }

        public RequeryResponse MapToRequeryResponse(UdaraTsqResponseModel? source)
        {
            if (source is null)
            {
                return new RequeryResponse
                {
                    IsSuccessful = false,
                    ResponseMessage = "Requery Failed",
                    Status = "Failed"
                };
            }

            var processingStatus = source.Data?.ProcessingStatus ?? string.Empty;

            // Only "Processed" means the transfer is confirmed successful
            var isSuccessful = source.Status &&
                               processingStatus.Equals("Processed", StringComparison.OrdinalIgnoreCase);

            return new RequeryResponse
            {
                IsSuccessful = isSuccessful,
                ResponseMessage = !string.IsNullOrWhiteSpace(source.Data?.ResponseMessage)
                    ? source.Data.ResponseMessage
                    : source.Message,
                ResponseCode = source.Data?.ResponseCode,          // for compatibility 
                Reference = source.Data?.TransactionReference,
                Status = processingStatus                          //  real status
            };
        }

        public GetCustomerCardResponse MapToGetCustomerCardResponse(UdaraGetCardAccountResponseModel? source)
        {
            if (source is null || !source.Status || source.Data?.Data is null)
            {
                return new GetCustomerCardResponse
                {
                    IsSuccessful = false,
                    ResponseDescription = source?.Message ?? "No cards found.",
                    Cards = null
                };
            }

            var cards = source.Data.Data.Select(item => new Card
            {
                AccountNumber = item.AccountNumber ?? item.Card?.AccountNumber,
                CardPAN = item.Card?.MaskedPan,
                LinkedDate = (item.Card?.IssuedDate ?? item.Card?.RequestDate) ?? DateTime.MinValue,
                ExpiryDate = item.Card?.ExpiryDate ?? DateTime.MinValue,
                SerialNo = item.Card?.SerialNumber,
                NameOnCard = item.NameOnCard ?? item.Card?.NameOnCard,
                Status = item.Status ?? item.Card?.Status
            }).ToArray();

            return new GetCustomerCardResponse
            {
                IsSuccessful = true,
                ResponseDescription = source.Message ?? "Cards retrieved successfully.",
                Cards = cards
            };
        }

    }
}
