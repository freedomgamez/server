using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DigitalWorldOnline.Infraestructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public const string DatabaseConnectionKey = "Database:Connection";

        public static IServiceCollection AddDsoDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var cs = configuration[DatabaseConnectionKey]
                ?? throw new InvalidOperationException(
                    $"Configuration value '{DatabaseConnectionKey}' is missing. " +
                    "Set it in appsettings.Development.json or via the DSO_Database__Connection environment variable.");

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseDsoMySql(cs);
            });
            return services;
        }

        internal static DbContextOptionsBuilder UseDsoMySql(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));
        }
    }
}
