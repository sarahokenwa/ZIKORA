using Aornis;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Repositories;

public interface IValidationLogRepository
{
    Task<Optional<ValidationLog>> GetByValidationReference(string validationReference);
}