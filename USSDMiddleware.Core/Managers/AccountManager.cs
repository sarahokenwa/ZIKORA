using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Accounts;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;

namespace USSDMiddleware.Core.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UssdProviderSelector _providerSelector;
        private readonly IUserRepository _userRepository;
        private readonly IValidationLogRepository _validationLogRepository;
        private readonly IProviderManager _providerManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountManager> _log;
        private readonly IPayOutService _payOutService;
        private readonly IConfiguration _configuration;
        private readonly IUserManager _userManager;
        private readonly IBlockAccountRepository _blockAccountRepository;



        public AccountManager(
            UssdProviderSelector providerSelector,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<AccountManager> log,
            IValidationLogRepository validationLogRepository,
            IProviderManager providerManager,
            IPayOutService payOutService,
            IConfiguration configuration,
            IUserManager userManager,
            IBlockAccountRepository blockAccountRepository)
        {
            _providerSelector = providerSelector;
            _userRepository = userRepository;
            _mapper = mapper;
            _log = log;
            _validationLogRepository = validationLogRepository;
            _providerManager = providerManager;
            _payOutService = payOutService;
            _configuration = configuration;
            _userManager = userManager;
            _blockAccountRepository = blockAccountRepository;
        }

        public async Task<CreateAccountResponse> CreateAccount(CreateAccountRequestExtension request)
        {
            try
            {
                var provider = _providerSelector.GetProvider(request.provider);
                var providerId = await provider.GetProviderId(_providerManager);

                //Get information from validation reference passed
                var validationLog =
                    (await _validationLogRepository.GetByValidationReference(request.ValidationReference))
                    .OrElseThrow(() =>
                        new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Validation reference is invalid!"));

                //Check if this user is already registered for USSD
                var user = await _userRepository.GetByPhoneNumber(validationLog.PhoneNumber, providerId);
                if (user.HasValue)
                {
                    return new CreateAccountResponse(request.ValidationReference, validationLog.PhoneNumber, null);

                }

                byte[] salt = Utility.GetSalt();
                var configuration = new ConfigurationBuilder().Build();
                var model = BuildUtil.BuildAccountCreationRequest(validationLog, configuration);
                model.Gender = request.Gender; model.Email = validationLog.Email; model.AccountOfficerCode = _configuration["ApiOptions:Zikora:AccountOfficerCode"]; model.ProductCode = _configuration["ApiOptions:Zikora:ProductCode"];
                AccountCreationResponse response = await provider.CreateAccount(model);
                var createdUser = await _userRepository.CreateUser(Builder<User>.CreateNew()
                    .With(u => u.Email = validationLog.Email)
                    .With(u => u.CustomerId = response.CustomerId)
                    .With(u => u.ProviderId = providerId)
                    .With(u => u.CustomerName = response.FullName)
                    .With(u => u.PhoneNumber = validationLog.PhoneNumber)
                    .With(u => u.Salt = Convert.ToBase64String(salt))
                    .With(u => u.TransactionPin = request.TransactionPin.EncryptTransactionPin(salt))
                    .With(u => u.DateOfBirth = validationLog.Dob)
                    .With(u => u.BankVerificationNumber = validationLog.Bvn)
                      .With(u => u.Address = "NA")
                    .Build());

                return new CreateAccountResponse(request.ValidationReference, validationLog.PhoneNumber, createdUser.Id);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to creating account");
                return new CreateAccountResponse(request.ValidationReference, null, null); 

            }
        }

        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest request)
        {
            try
            {
                var nameEnquiry = await _payOutService.NameEnquiry(request);

                return nameEnquiry;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to verify account name.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Name enquiry failed.");
            }
        }

        public async Task<GetUserByAccountNumberResponse> GetUserByAccountNumber(AccountValidationRequest request)
        {
            try
            {
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                var accountValidationRequest = new AccountValidationRequest
                {
                    AccountNumber = request.AccountNumber,

                };

                GetUserByAccountNumberResponse getUserByAccountNumberResponse = await provider.GetUserByAccountNumber(accountValidationRequest.AccountNumber);
                if (getUserByAccountNumberResponse == null)
                {
                    throw new NotFoundException($"User with {request.AccountNumber} doesn't exist.");
                }

                return getUserByAccountNumberResponse;

            }
            catch (Exception ex)
            {
                _log.LogError(ex, "User not found.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "User not found.");
            }
        }

        public async Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request)
        {

            try
            {
                if (string.IsNullOrEmpty(request.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "AccountNo is required.",
                        ResponseStatus = "Failed"
                    };
                }

                if (string.IsNullOrEmpty(request.OwnersPhoneNumber))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Owners phone number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                if (string.IsNullOrEmpty(request.RequestPhoneNumber))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Request phone number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                if (string.IsNullOrEmpty(request.Pin))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }


                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                var freezeAccount = new BlockAccount
                {
                    OwnersPhoneNumber = request.OwnersPhoneNumber,
                    RequestPhoneNumber = request.RequestPhoneNumber,
                    AccountNo = request.AccountNo,
                    ProviderId = providerId
                };

                BlockAccount logBlockAccountRequest = await LogBlockAccount(freezeAccount);

                var phoneValidationRequest = new PhoneValidationRequest
                {
                    PhoneNumber = request.OwnersPhoneNumber,
                    Provider = request.Provider
                };

                var user = await _userManager.GetUserByPhoneNumber(phoneValidationRequest);

                if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "User not found.",
                        ResponseStatus = "Failed"
                    };
                }

                var userPin = await _userManager.ValidateTransactionPin(request.Pin, request.OwnersPhoneNumber, providerId);
                if (!userPin)
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "The pin entered is incorrect.",
                        ResponseStatus = "Failed"
                    };
                }


                var blockAccountRequest = new BlockAccountRequest
                {
                    AccountNo = request.AccountNo,

                };

                BlockAccountResponse blockAccountResponse = await provider.BlockAccount(blockAccountRequest);

                BlockAccount updateBlockAccount = await UpdateBlockAccount(blockAccountResponse, logBlockAccountRequest, providerId);

                return blockAccountResponse;

            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while trying to block account.", ex);
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<BlockAccount> LogBlockAccount(BlockAccount request)
        {
            return await _blockAccountRepository.LogBlockAccount(Builder<BlockAccount>.CreateNew()
              .With(b => b.OwnersPhoneNumber = request.OwnersPhoneNumber)
              .With(d => d.RequestPhoneNumber = request.RequestPhoneNumber)
              .With(d => d.ProviderId = request.ProviderId)
              .With(d => d.AccountNo = request.AccountNo)
              .With(d => d.CreatedOn = DateTime.Now)
            .With(d => d.UpdatedOn = DateTime.Now)
            .Build());

        }

        public async Task<BlockAccount> UpdateBlockAccount(BlockAccountResponse blockAccountResponse, BlockAccount logBlockAccountRequest, string providerId)
        {

            logBlockAccountRequest.ResponseStatus = blockAccountResponse.ResponseStatus;
            logBlockAccountRequest.ResponseDescription = blockAccountResponse.ResponseDescription;
            logBlockAccountRequest.RequestStatus = blockAccountResponse.RequestStatus;

            return await _blockAccountRepository.UpdateBlockAccount(logBlockAccountRequest, providerId);

        }

        public async Task<BlockAccountResponse> DeactivatePND(BlockAccountRequest request)
        {

            try
            {

                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                if (string.IsNullOrEmpty(request.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                var deactivatePND = await provider.DeactivatePND(request);

                if (deactivatePND.ResponseStatus == "Failed")
                {
                    throw new NotSuccessfulException("PND deactivation failed.");
                }

                return deactivatePND;

            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while trying to deactivate PND.", ex);
                throw new NotSuccessfulException(ex.Message);
            }
        }

        public async Task<BlockAccountResponse> VerifyPNDStatus(BlockAccountRequest request)
        {

            try
            {
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                if (string.IsNullOrEmpty(request.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus = false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                var verifyPNDStatus = await provider.VerifyPNDStatus(request);

                if (verifyPNDStatus.ResponseStatus == "Failed")
                {
                    throw new NotSuccessfulException("PND status verification failed.");
                }

                return verifyPNDStatus;
            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while trying to verify PND status.", ex);
                throw new NotSuccessfulException(ex.Message);
            }
        }


    }
}





