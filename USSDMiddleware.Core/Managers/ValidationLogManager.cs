using Aornis;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Exceptions;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;

namespace USSDMiddleware.Core.Managers;

public class ValidationLogManager : IValidationLogManager
{
    private readonly IValidationLogRepository _validationLogRepository;

    public ValidationLogManager(IValidationLogRepository validationLogRepository)
    {
        _validationLogRepository = validationLogRepository;
    }


    public async Task<ValidationLog> CreateValidationLog(ValidationLog validationLog)
    {
        return await _validationLogRepository.CreateValidationLog(validationLog);
    }

    public async Task<ValidationLog> GetValidationLogByReference(string reference)
    {
        var validationLog = await _validationLogRepository.GetByValidationReference(reference);
        
        if (!validationLog.HasValue)
        {
            throw new NotFoundException($"No information was found for this reference: {reference}");
        }

        return validationLog.Value;
    }
}