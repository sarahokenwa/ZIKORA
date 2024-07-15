using Aornis;
using Microsoft.EntityFrameworkCore;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Infrastructure.Repositories;

public class ValidationLogRepository : IValidationLogRepository
{
    private readonly DataEntities _dbContext;

    public ValidationLogRepository(DataEntities dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Optional<ValidationLog>> GetByValidationReference(string validationReference)
    {
        return Optional.Of(await _dbContext.ValidationLogs.FirstOrDefaultAsync(v => v.ValidationReference == validationReference));
    }
}