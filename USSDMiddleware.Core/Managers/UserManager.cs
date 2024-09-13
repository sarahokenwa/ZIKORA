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

namespace USSDMiddleware.Core.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly UssdProviderSelector _providerSelector;
        private readonly IProviderManager _providerManager;

        public UserManager(
            IUserRepository userRepository,
            UssdProviderSelector providerSelector,
            IProviderManager providerManager
           )

        {
            _userRepository = userRepository;
            _providerSelector = providerSelector;
            _providerManager = providerManager;
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
                throw new AlreadyExistException($"User with {request.PhoneNumber} already exist");
            }

            byte[] salt = Utility.GetSalt();
            var serviceRsp = await provider.GetUserByPhoneNumber(request.PhoneNumber);
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
            return new List<UserAccountNumber>();
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
                    Message = "Invalid phone number or pin",
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


