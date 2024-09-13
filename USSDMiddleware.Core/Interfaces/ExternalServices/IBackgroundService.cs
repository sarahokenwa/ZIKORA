using System.Linq.Expressions;

namespace USSDMiddleware.Core.Interfaces.ExternalServices
{
    public interface IBackgroundService
    {
        Task EnqueueProcess(Expression<Func<Task>> methodCall);
        Task ScheduleProcess(Expression<Func<Task>> methodCall, TimeSpan delay);
       
    }
}
