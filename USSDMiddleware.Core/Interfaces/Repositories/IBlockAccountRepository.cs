using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories
{
    public interface IBlockAccountRepository
    {
        Task<BlockAccount> LogBlockAccount(BlockAccount request);
        Task<BlockAccount> UpdateBlockAccount(BlockAccount model, string providerId);
    }
}
