using AutoMapper;
using FizzWare.NBuilder;
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


        public AccountManager(
            UssdProviderSelector providerSelector,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<AccountManager> log,
            IValidationLogRepository validationLogRepository,
            IProviderManager providerManager,
            IPayOutService payOutService)
        {
            _providerSelector = providerSelector;
            _userRepository = userRepository;
            _mapper = mapper;
            _log = log;
            _validationLogRepository = validationLogRepository;
            _providerManager = providerManager;
            _payOutService = payOutService;
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
                var model = BuildUtil.BuildAccountCreationRequest(validationLog);
                model.Email = request.Email; model.AccountOfficerCode = request.AccountOfficerCode;model.ProductCode = request.ProductCode; model.Gender = request.Gender;
                var response = await provider.CreateAccount(model);
                var createdUser = await _userRepository.CreateUser(Builder<User>.CreateNew()
                    .With(u => u.Address = "")
                    .With(u => u.Email = "")
                    .With(u => u.CustomerId = response.CustomerId)
                    .With(u => u.ProviderId = providerId)
                    .With(u => u.CustomerName = response.FullName)
                    .With(u => u.PhoneNumber = validationLog.PhoneNumber)
                    .With(u => u.TransactionPin = request.TransactionPin)
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

    }
}





