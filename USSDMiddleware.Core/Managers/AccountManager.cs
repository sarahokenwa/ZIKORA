using AutoMapper;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Enums;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;

namespace USSDMiddleware.Core.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UssdProviderSelector _providerSelector;
        private readonly IAccountRepository _accountRepository;
        private readonly IValidationLogRepository _validationLogRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountManager> _log;


        public AccountManager(
            UssdProviderSelector providerSelector,
            IAccountRepository accountRepository,
            IMapper mapper,
            ILogger<AccountManager> log,
            IValidationLogRepository validationLogRepository) 
        { 
            _providerSelector = providerSelector;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _log = log;
            _validationLogRepository = validationLogRepository;
        }

        public async Task<AccountCreationResponse> CreateAccount(CreateAccountRequest request)
        {
            try
            {
                var validationLog =
                    (await _validationLogRepository.GetByValidationReference(request.ValidationReference))
                    .OrElseThrow(() =>
                        new UssdMiddlewareException(ExceptionType.BAD_REQUEST, "Validation reference is invalid!"));

                var response = await _providerSelector.GetProvider(request.provider)
                    .CreateAccount(BuildUtil.BuildAccountCreationRequest(validationLog));
             
                var newAccount = _mapper.Map<Account>(response);
                await _accountRepository.CreateNewAccount(newAccount);
                return response;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while trying to creating account");
                throw new UssdMiddlewareException(ExceptionType.OPERATION_FAILED,"Account creation failed.");
            }
        }
    }
}

   



    