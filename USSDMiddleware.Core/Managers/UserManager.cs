using FizzWare.NBuilder;
using Hangfire.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;
using USSDMiddleware.Infrastructure.Entities;

namespace USSDMiddleware.Core.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly UssdProviderSelector _providerSelector;
        private readonly IProviderManager _providerManager;
        private readonly ILogger<UserManager> _log;
        private readonly ApiOptions _apiOptions;

        public UserManager(
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            ILogger<UserManager> log,
            ApiOptions apiOptions
           )

        {
            _userRepository = userRepository;
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _log = log;
            _apiOptions = apiOptions;
        }

        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            request.PhoneNumber = ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
                .With(v => v.PhoneNumber = request.PhoneNumber)
                .Build());

            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);

            var user = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

            if (user.HasValue)
            {
                return new CreateUserResponse
                {
                    userId = null,
                    message = $"User with phone number {request.PhoneNumber} already exists."
                };
            }

            byte[] salt = Utility.GetSalt();
            string saltBase64 = Convert.ToBase64String(salt);
            _log.LogInformation($"Generated Salt (Base64): {saltBase64}");

            if (salt == null || salt.Length == 0)
            {
                throw new InvalidOperationException("Salt generation failed. The byte array is null or empty.");
            }

            var serviceRsp = await provider.GetUserByPhoneNumber(request.PhoneNumber);
            if (serviceRsp.Message?.Contains("No customer found with PhoneNumber") == true)
            {
                return new CreateUserResponse
                {
                    message = $"No customer found with PhoneNumber: {request.PhoneNumber}."
                };
            }

            var createdUser = await _userRepository.CreateUser(Builder<User>.CreateNew()
                .With(u => u.PhoneNumber = request.PhoneNumber)
                .With(u => u.Address = serviceRsp.Address)
                .With(u => u.Email = serviceRsp.Email)
                .With(u => u.CustomerId = serviceRsp.CustomerID)
                .With(u => u.CustomerName = $"{serviceRsp.LastName}{serviceRsp.OtherNames}")
                .With(u => u.Salt = Convert.ToBase64String(salt))
                .With(u => u.TransactionPin = request.TransactionPin.EncryptTransactionPin(salt))
                .With(u => u.ProviderId = providerId)
                .With(u => u.BankVerificationNumber = serviceRsp.BankVerificationNumber)

                .Build());

            return Builder<CreateUserResponse>.CreateNew()
                .With(c => c.userId = createdUser.Id)
                .With(c => c.message = "Successful")
                .Build();
        }

        public async Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request)
        {
            request.PhoneNumber = ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
                .With(v => v.PhoneNumber = request.PhoneNumber)
                .Build());

            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);
            var user = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

            if (user.HasValue)
            {
                return new PhoneValidationResponse(true, false, "Successful");

            }

            return await provider.ValidatePhone(request);
        }

        //For existing ZIKORA customers.
        public async Task<UserPhoneNumberDetails> GetUserByPhoneNumber(PhoneValidationRequest request)
        {
            ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
                .With(v => v.PhoneNumber = request.PhoneNumber)
                .Build());

            var provider = _providerSelector.GetProvider(request.Provider);
            var serviceRsp = await provider.GetUserByPhoneNumber(request.PhoneNumber);

            if (serviceRsp != null)
            {
                return new UserPhoneNumberDetails
                {
                    DateOfBirth = serviceRsp.DateOfBirth,
                    BankVerificationNumber = serviceRsp.BankVerificationNumber,
                    Email = serviceRsp.Email,
                    PhoneNumber = serviceRsp.PhoneNumber,
                };

            }
            return new UserPhoneNumberDetails();

        }

        public async Task<List<UserAccountNumber>> GetAccountsByPhoneNumber(PhoneValidationRequest request)
        {
            ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
                .With(v => v.PhoneNumber = request.PhoneNumber)
                .Build());

            var provider = _providerSelector.GetProvider(request.Provider);
            var serviceRsp = await provider.GetAccountsByPhoneNumber(request.PhoneNumber);

            if (serviceRsp.Count > 0 && serviceRsp.Any(x => x.AccountNumber != null))
            {
                return serviceRsp.Select(x => new UserAccountNumber
                {
                    AccountNumber = x.AccountNumber,
                    Message = "Account retrieved successfully."
                }).ToList();

            }
            return new List<UserAccountNumber>
    {
        new UserAccountNumber
        {
            Message = serviceRsp.Count > 0 ? serviceRsp.First().Message : $"No customer found with phone number {request.PhoneNumber}"
        }
    };
        }

        public async Task<AccountBalanceEnquiry> GetAccountBalance(AccountRequest request)
        {
            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);

            var userDetail = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);

            var userPin = await ValidateTransactionPin(request.TransactionPin, request.PhoneNumber, providerId);
            if (!userPin)
            {
                return new AccountBalanceEnquiry
                {
                    Message = "The pin entered is incorrect",
                    Status = false,
                };
            }

            var serviceRsp = await provider.CheckAccountBalance(new BalanceEnquiryRequest { AccountNumber = request.AccountNumber });

            if (serviceRsp != null)
            {
                return new AccountBalanceEnquiry
                {
                    Status = true,
                    Message = "Successful",
                    AvailableBalance = serviceRsp.AvailableBalance,
                    WithdrawableBalance = serviceRsp.WithdrawableBalance
                };


            }
            return new AccountBalanceEnquiry
            {
                Status = false,
                Message = "Failed to retrieve balance",
            };

        }

        public async Task<bool> ValidateTransactionPin(string transactionPin, string phoneNumber, string providerId)
        {
            var user = await _userRepository.GetByPhoneNumber(phoneNumber, providerId);
            if (user == null)
            {
                return false;
            }
            byte[] salt = Convert.FromBase64String(user.Value.Salt);
            string pin = transactionPin.HashSecret(salt);

            if (!user.Value.TransactionPin.Equals(pin))
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        // Initiate Pin rset for user by generating an OTP and sending it to the user's phone number
        public async Task<PinResetResponse> InitiatePinReset(PinResetRequest request)
        {
            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);
            var user = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);
            if (user == null)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = $"User with phone number {request.PhoneNumber} does not exist."
                };
            }
            // Generate OTP
            string otp = Utility.GenerateRandomDigits(6);
            _log.LogInformation($"Generated OTP: {otp}");
            // Savd the OTP in the database for later varification.
            OTPLog otpLog = new OTPLog
            {
                PhoneNumber = request.PhoneNumber,
                OTP = otp,
                ProviderId = providerId,
                CreatedOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_apiOptions.Zikora.OTPValidityInMinutes)
            };

            await _userRepository.LogNewteOTP(otpLog);

            _log.LogInformation($"Sending OTP to {request.PhoneNumber}: {otp}");

            string sms = _apiOptions.Zikora.SMSMessageTemplate.Replace("{OTP}", otp)
                .Replace("{Validity}", _apiOptions.Zikora.OTPValidityInMinutes.ToString());

            var smsResponse = await provider.SendSMS(new SMSRequest
            {
                PhoneNumber = request.PhoneNumber,
                Message = sms
            });

            if (smsResponse == null || !smsResponse.IsSuccess)
            {
                _log.LogError($"Failed to send OTP SMS to {request.PhoneNumber}. Response: {smsResponse?.ResponseMessage}");
                return new PinResetResponse
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again later."
                };
            }

            return new PinResetResponse
            {
                Success = smsResponse?.IsSuccess ?? false,
                Message = _apiOptions.Zikora.ResetInstruction
            };
        }

        // Verify the OTP provided by the user and reset the transaction pin if the OTP is valid
        public async Task<PinResetResponse> VerifyOTPAndResetPin(CompletePinResetRequest request)
        {
            var provider = _providerSelector.GetProvider(request.Provider);
            var providerId = await provider.GetProviderId(_providerManager);
            var user = await _userRepository.GetByPhoneNumber(request.PhoneNumber, providerId);
            if (user == null)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = $"User with phone number {request.PhoneNumber} does not exist."
                };
            }

            var otpLog = await _userRepository.GetLatestOTPLog(request.PhoneNumber, providerId);

            if (otpLog == null)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = "No OTP request found for this phone number. Please initiate a pin reset request first."
                };
            }

            if (otpLog.IsUsed || otpLog.ExpiresOn < DateTimeOffset.UtcNow)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = "The OTP is either already used or has expired."
                };
            }
            if (otpLog.OTP != request.OTP)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = "The OTP entered is incorrect."
                };
            }
            // Mark the OTP as used
            byte[] salt = Convert.FromBase64String(user.Value.Salt);
            string pin = request.NewPin.HashSecret(salt);

            
            // Reset the user's transaction pin
            var resetResult = await _userRepository.UpdateUserPinAndMarkOTPAsUsed(user.Value, pin, otpLog);
            if (!resetResult)
            {
                return new PinResetResponse
                {
                    Success = false,
                    Message = "Failed to reset transaction pin. Please try again."
                };
            }
            return new PinResetResponse
            {
                Success = true,
                Message = "Transaction pin reset successful."
            };
        }
    }
}


