using Newtonsoft.Json;
using System.Net.Http.Headers;
using USSDMiddleware.Core.Interfaces.Component;
using USSDMiddleware.Core.Interfaces.ExternalServices;
using USSDMiddleware.Core.Interfaces.Managers;
using USSDMiddleware.Core.Interfaces.Providers;
using USSDMiddleware.Core.Interfaces.Repositories;
using USSDMiddleware.Core.Managers;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.V2;
using USSDMiddleware.Core.Models.V2.Mapper;
using USSDMiddleware.Core.Services;
using USSDMiddleware.Core.Utilities;
using USSDMiddleware.Infrastructure.Providers;
using USSDMiddleware.Infrastructure.Repositories;

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
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IBlockAccountRepository, BlockAccountRepository>();
            services.AddScoped<IIntraBankTransferRepository, IntraBankTransferRepository>();

            //Third party service
            #region service
            services.AddHttpClient<IHttpService, HttpServiceUtil>();
            services.AddScoped<IUssdProvider, ZikoraBankOneProvider>();
            services.AddScoped<ICyberPayProvider, CyberPayProvider>();
            services.AddScoped<UssdProviderSelector>();
            services.AddScoped<IPayOutService, PayOutService>();
            services.AddScoped<IBackgroundService, HangfireBackgroundService>();
          //  services.AddScoped<IUssdProvider, ZikoraUdaraProvider>();

            // services.AddSingleton<ILogService, SerilogService>();

            #endregion service

            //Configurations

            // services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));

            services.AddSingleton(configuration.GetSection("ApiOptions").Get<ApiOptions>());

            // Register CyberPay API client
            var zikoraSMSBaseUrl = configuration["ApiOptions:Zikora:SMSProviderUrl"];
            if (string.IsNullOrWhiteSpace(zikoraSMSBaseUrl) || !Uri.TryCreate(zikoraSMSBaseUrl, UriKind.Absolute, out var zikoraSMSUri))
            {
                throw new InvalidOperationException("Configuration 'ApiOptions:Zikora:SMSProviderUrl' is missing or is not a valid absolute URI.");
            }

            services.AddHttpClient("SMSProvider", client =>
            {
                client.BaseAddress = new Uri(zikoraSMSBaseUrl);
                // add custom headers
                //client.DefaultRequestHeaders.Add("IntegrationKey", configuration["Zikora:CyberPay:IntegrationKey"] ?? string.Empty);
                //client.DefaultRequestHeaders.Add("ApiKey", configuration["Zikora:CyberPay:ApiKey"] ?? string.Empty);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var hostEnvironment = (IWebHostEnvironment)serviceProvider.GetService(typeof(IWebHostEnvironment));
            services.AddDistributedMemoryCache();

            services.Configure<UdaraOptions>(configuration.GetSection(UdaraOptions.SectionName));

            services.AddSingleton<UdaraMapper>();

            services.AddHttpClient<ZikoraUdaraProvider>(client =>
            {
                client.BaseAddress = new Uri(configuration["Udara:BaseUrl"]!);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });
        }

        public static T ToModel<T>(this T value, string val)
        {
            return JsonConvert.DeserializeObject<T>(val);
        }
    }
}
