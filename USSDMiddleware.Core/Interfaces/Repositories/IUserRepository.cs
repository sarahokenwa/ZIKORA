using Aornis;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<Optional<User>> GetByPhoneNumber(string phoneNumber, string providerId);
        Task<User> CreateUser(User user);
        Task<OTPLog?> LogNewteOTP(OTPLog otpLog);
        Task<OTPLog?> GetLatestOTPLog(string phoneNumber, string providerId);
        Task<bool> UpdateUserPinAndMarkOTPAsUsed(User user, string newPin, OTPLog otpLog);
    }
}
