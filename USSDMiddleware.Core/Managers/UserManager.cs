using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Utilities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Interfaces.Managers;
using FizzWare.NBuilder;
using USSDMiddleware.Core.Models.Request;
using USSDMiddleware.Core.Models.ResponseModel;


namespace USSDMiddleware.Core.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly UssdProviderSelector _providerSelector;

        public UserManager(
            IUserRepository userRepository,
            UssdProviderSelector providerSelector
           )

        {
            _userRepository = userRepository;
            _providerSelector = providerSelector;
        }

        public async Task<PhoneValidationResponse> ValidatePhone(PhoneValidationRequest request)
        {
            ValidationUtil.Validate(Builder<ValidationModel>.CreateNew()
                .With(v => v.PhoneNumber = request.PhoneNumber)
                .Build());

            var user = await _userRepository.GetByPhoneNumber(request.PhoneNumber);
            if (user.HasValue)
            {
                return new PhoneValidationResponse(true, false, "Successful");
            }

            var validator = _providerSelector.GetProvider(request.Provider);

            return await validator.ValidatePhone(request);
        }
    }
}


