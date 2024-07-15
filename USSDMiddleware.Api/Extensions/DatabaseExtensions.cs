using Microsoft.EntityFrameworkCore;
using USSDMiddleware.Infrastructure.Data;

namespace USSDMiddleware.Api.Extensions
{
    public static class DatabaseExtension
    {
        public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataEntities>(options =>
              options.UseSqlServer(
                  configuration.GetConnectionString("DataEntities"), b => b.MigrationsAssembly("USSDMiddleware.Api")));

            services.AddScoped<DbContext, DataEntities>();

            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = "RedisConnection";
            //    options.InstanceName = "master";
            //});

        }
    }
}
