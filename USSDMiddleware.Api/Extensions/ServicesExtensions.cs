using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Managers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Infrastructure.Repositories;
using USSDMiddleware.Infrastructure.Providers;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;
using Newtonsoft.Json;
using USSDMiddleware.Core.Interfaces.ExternalServices;

namespace USSDMiddleware.Api.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            //Managers
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IAccountManager, AccountManager>();
            services.AddScoped<IBvnManager, BvnManager>();
            services.AddScoped<IBillsManager, BillsManager>();
            services.AddScoped<IProviderManager, ProviderManager>();
            services.AddScoped<IValidationLogManager, ValidationLogManager>();
            services.AddScoped<IPayOutManager, PayOutManager>();
            services.AddScoped<ICardManager, CardManager>();
            


            //Repositories
            #region Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            
            #endregion
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IValidationLogRepository, ValidationLogRepository>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<IBillsRepository, BillsRepository>();
            services.AddScoped<IInstantPayOutRepository, InstantPayOutRepository>();
            services.AddScoped<ICustomerDebitRepository, CustomerDebitRepository>();

            //Third party service
            #region service
            services.AddHttpClient<IHttpService, HttpServiceUtil>();
            services.AddScoped<IUssdProvider, ZikoraProvider>();
            services.AddScoped<ICyberPayProvider, CyberPayProvider>();
            services.AddScoped<UssdProviderSelector>();
            services.AddScoped<IPayOutService, PayOutService>();



            // services.AddSingleton<ILogService, SerilogService>();

            #endregion service

            //Configurations

            services.AddSingleton(configuration.GetSection("ApiOptions").Get<ApiOptions>());

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var hostEnvironment = (IWebHostEnvironment)serviceProvider.GetService(typeof(IWebHostEnvironment));
            services.AddDistributedMemoryCache(); 
        }

        public static T ToModel<T>(this T value, string val)
        {
            return JsonConvert.DeserializeObject<T>(val);
        }
    }
}
