using System.Linq.Expressions;
using Hangfire;
using USSDMiddleware.Core.Interfaces.ExternalServices;

namespace USSDMiddleware.Core.Services
{
    public class HangfireBackgroundService : IBackgroundService
    {
       
        public async Task EnqueueProcess(Expression<Func<Task>> methodCall)
        {
            await Task.Factory.StartNew(() => BackgroundJob.Enqueue((methodCall)));
        }

        public async Task ScheduleProcess(Expression<Func<Task>> methodCall, TimeSpan delay)
        {
            await Task.Factory.StartNew(() => BackgroundJob.Schedule((methodCall), delay));
        }
    }
}
