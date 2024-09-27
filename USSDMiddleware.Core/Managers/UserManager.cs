using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Utilities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Interfaces.Managers;
using FizzWare.NBuilder;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Models.Accounts;
using Microsoft.Extensions.Logging;

namespace USSDMiddleware.Core.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly UssdProviderSelector _providerSelector;
        private readonly IProviderManager _providerManager;
        private readonly ILogger<UserManager> _log;

        public UserManager(
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager,
            ILogger<UserManager> log
           )

        {
            _userRepository = userRepository;
            _providerSelector = providerSelector;
            _providerManager = providerManager;
            _log = log;
        }

        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
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
            ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
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

            if (serviceRsp.Count > 0)
            {
                return serviceRsp.Select(x => new UserAccountNumber
                {
                    AccountNumber = x.AccountNumber
                }).ToList();

            }
            return null;
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
                    Balance = "",
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
                    Balance = serviceRsp.WithdrawableBalance
                };


            }
            return new AccountBalanceEnquiry
            {
                Status = false,
                Message = "Failed to retrieve balance",
                Balance = null
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
    }
}


