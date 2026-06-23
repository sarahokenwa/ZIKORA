using Aornis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataEntities _dbContext;
        private readonly ILogger<AccountRepository> _log;


        public UserRepository(DataEntities dbContext, ILogger<AccountRepository> log)
        {
            _dbContext = dbContext;
            _log = log;
        }


        public Task<Optional<User>> GetByPhoneNumber(string phoneNumber, string providerId)
        {
            return Task.FromResult(Optional.Of(_dbContext.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.ProviderId == providerId)));
        }

        public async Task<User> CreateUser(User user)
        {
            try
            {
                var newUser = await _dbContext.Users.AddAsync(user);

                await _dbContext.SaveChangesAsync();

                return newUser.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to save the new user with phoneNumber: {user.PhoneNumber}");
                return null;
            }
        }

        // Update user PIN
        public async Task<User?> UpdateUserPin(User user, string newPin)
        {
            try
            {
                user.TransactionPin = newPin;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to update the PIN for user with phoneNumber: {user.PhoneNumber}");
                return null;
            }
        }

        public async Task<OTPLog?> LogNewteOTP(OTPLog otpLog)
        {
            try
            {
                var newOtpLog = await _dbContext.OTPLogs.AddAsync(otpLog);
                await _dbContext.SaveChangesAsync();
                return newOtpLog.Entity;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to save the new OTP log for phoneNumber: {otpLog.PhoneNumber}");
                return null;
            }
        }

        public async Task<OTPLog?> GetLatestOTPLog(string phoneNumber, string providerId)
        {
            try
            {
                var otpLog = await _dbContext.OTPLogs
                    .Where(log => log.PhoneNumber == phoneNumber && log.ProviderId == providerId)
                    .OrderByDescending(log => log.CreatedOn)
                    .FirstOrDefaultAsync();
                return otpLog;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to retrieve the latest OTP log for phoneNumber: {phoneNumber}");
                return null;
            }
        }

        public async Task<OTPLog?> MarkOTPAsUsed(OTPLog otpLog)
        {
            try
            {
                otpLog.IsUsed = true;
                otpLog.UsedOn = DateTimeOffset.UtcNow;
                _dbContext.OTPLogs.Update(otpLog);
                await _dbContext.SaveChangesAsync();
                return otpLog;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Failed to mark OTP log as used for phoneNumber: {otpLog.PhoneNumber}");
                return null;
            }
        }

        // Update transaction and mark OTP as used in a transaction
        public async Task<bool> UpdateUserPinAndMarkOTPAsUsed(User user, string newPin, OTPLog otpLog)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Update user PIN
                user.TransactionPin = newPin;
                _dbContext.Users.Update(user);
                // Mark OTP as used
                otpLog.IsUsed = true;
                otpLog.UsedOn = DateTimeOffset.UtcNow;
                _dbContext.OTPLogs.Update(otpLog);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _log.LogError(ex, $"Failed to update the PIN and mark OTP as used for user with phoneNumber: {user.PhoneNumber}");
                return false;
            }
        }
    }
}
