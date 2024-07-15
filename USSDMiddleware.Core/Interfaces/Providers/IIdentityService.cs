using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USSDMiddleware.Core.Models.Security;

namespace USSDMiddleware.Core.Interfaces.Providers
{
    public interface IIdentityService
    {
        Task<IdentityUserModel> CreateUser(IdentityUserModel userModel);
        Task<TokenModel> GetClientToken();
        Task<IdentityUserModel> GetUser(string emailAddress);
        Task<IdentityUserModel> GetUserById(string userId);
        Task<List<string>> GetUsersInPermission(string permissionName);
    }
}
