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


        public AccountManager(
            UssdProviderSelector providerSelector,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<AccountManager> log,
            IValidationLogRepository validationLogRepository,
            IProviderManager providerManager,
            IPayOutService payOutService,
            IConfiguration configuration)
        {
            _providerSelector = providerSelector;
            _userRepository = userRepository;
            _mapper = mapper;
            _log = log;
            _validationLogRepository = validationLogRepository;
            _providerManager = providerManager;
            _payOutService = payOutService;
            _configuration = configuration;
        }

        public async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest request)
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
                    throw new BadRequestException(
                        $"A user has already exist for this reference {validationLog.ValidationReference}");
                }
                byte[] salt = Utility.GetSalt();
                var configuration = new ConfigurationBuilder().Build(); 
                var model = BuildUtil.BuildAccountCreationRequest(validationLog, configuration);
                model.Gender = request.Gender; model.Email = validationLog.Email; model.AccountOfficerCode = _configuration["ApiOptions:Zikora:AccountOfficerCode"]; model.ProductCode = _configuration["ApiOptions:Zikora:ProductCode"];
                // model.Email = request.Email; model.AccountOfficerCode = request.AccountOfficerCode;model.ProductCode = request.ProductCode; 
                AccountCreationResponse response = await provider.CreateAccount(model);
                var createdUser = await _userRepository.CreateUser(Builder<User>.CreateNew()
                    .With(u => u.Address = "")
                    .With(u => u.Email = validationLog.Email)
                    //.With(u => u.Email = "")
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
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Account creation failed.");
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
                _log.LogError(ex, "An error occurred while trying to check account name.");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED, "Name enquiry failed.");
            }
        }

        public async Task<BlockAccountResponse> BlockAccount(BlockAccountRequest request)
        {

            try
            {
               
                var provider = _providerSelector.GetProvider(request.Provider);
                var providerId = await provider.GetProviderId(_providerManager);

                if (string.IsNullOrEmpty(request.AccountNo))
                {
                    return new BlockAccountResponse
                    {
                        RequestStatus= false,
                        ResponseDescription = "Account number is required.",
                        ResponseStatus = "Failed"
                    };
                }

                var blockAccount = await provider.BlockAccount(request);

                if (blockAccount.ResponseStatus == "Failed")
                {
                    throw new NotSuccessfulException("Account blocking was unsuccessful.");
                }

                return blockAccount;
            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while trying to block account.", ex);
                throw new NotSuccessfulException(ex.Message);
            }
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
            catch(Exception ex)
            {
                _log.LogError("An error occurred while trying to verify PND status.", ex);
                throw new NotSuccessfulException(ex.Message);
            }
        }


    }
}





