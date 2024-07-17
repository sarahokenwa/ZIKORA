using Aornis;
using USSDMiddleware.Core.Entities;

namespace USSDMiddleware.Core.Interfaces.Managers;

public interface IValidationLogManager
{
    Task<ValidationLog> CreateValidationLog(ValidationLog validationLog);
    Task<ValidationLog> GetValidationLogByReference(string reference);
}