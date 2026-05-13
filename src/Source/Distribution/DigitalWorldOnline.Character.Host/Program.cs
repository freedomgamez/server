using DigitalWorldOnline.Application.Admin.Repositories;
using DigitalWorldOnline.Application.CharacterAssets;
using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Application.CharacterAssets.Mapping;
using DigitalWorldOnline.Application.Extensions;
using DigitalWorldOnline.Application.Services;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Repositories.Admin;
using DigitalWorldOnline.Infraestructure;
using DigitalWorldOnline.Infraestructure.Extensions;
using DigitalWorldOnline.Infraestructure.Mapping;
using DigitalWorldOnline.Infraestructure.Repositories.Account;
using DigitalWorldOnline.Infraestructure.Repositories.Admin;
using DigitalWorldOnline.Infraestructure.Repositories.Character;
using DigitalWorldOnline.Infraestructure.Repositories.Config;
using DigitalWorldOnline.Infraestructure.Repositories.Routine;
using DigitalWorldOnline.Infraestructure.Repositories.Server;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Reflection;

namespace DigitalWorldOnline.Character
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Run();
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                Console.WriteLine("Terminating by unhandled exception...");
            else
                Console.WriteLine("Received unhandled exception.");

            Console.ReadLine();
        }

        public static IHost CreateHostBuilder(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseEnvironment("Development")
                .UseDefaultServiceProvider((_, options) =>
                {
                    options.ValidateOnBuild = false;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<StoragePersistenceDualWriteCoordinator>();

                    services.AddDsoDatabase(context.Configuration);

                    services.AddScoped<IAdminQueriesRepository, AdminQueriesRepository>();
                    services.AddScoped<IAdminCommandsRepository, AdminCommandsRepository>();

                    services.AddScoped<IAccountQueriesRepository, AccountQueriesRepository>();
                    services.AddScoped<IAccountCommandsRepository, AccountCommandsRepository>();

                    services.AddScoped<IServerQueriesRepository, ServerQueriesRepository>();
                    services.AddScoped<IServerCommandsRepository, ServerCommandsRepository>();

                    services.AddScoped<ICharacterQueriesRepository, CharacterQueriesRepository>();
                    services.AddScoped<ICharacterCommandsRepository, CharacterCommandsRepository>();

                    services.AddScoped<IConfigQueriesRepository, ConfigQueriesRepository>();
                    services.AddScoped<IConfigCommandsRepository, ConfigCommandsRepository>();

                    services.AddScoped<IRoutineRepository, RoutineRepository>();

                    //services.AddScoped<IEmailService, EmailService>();

                    services.AddSingleton<ISender, ScopedSender<Mediator>>();
                    services.AddSingleton<IProcessor, CharacterPacketProcessor>();
                    services.AddSingleton(ConfigureLogger(context.Configuration));

                    // Static-data bin loaders (loaded eagerly below after Build())
                    services.AddSingleton<CharCreateTableBinLoader>();
                    services.AddSingleton<DMBaseBinLoader>();
                    services.AddSingleton<DigimonListBinLoader>();
                    services.AddSingleton<DigimonEvoBinLoader>();

                    services.AddHostedService<CharacterServer>();
                    services.AddMediatR(
                        typeof(MediatorApplicationHandlerExtension).GetTypeInfo().Assembly,
                        typeof(CharacterAssetsMarker).GetTypeInfo().Assembly);
                    services.AddTransient<Mediator>();

                    services.AddAutoMapper(typeof(AccountProfile));
                    services.AddAutoMapper(typeof(CharacterAssetsProfile));
                    services.AddAutoMapper(typeof(CharacterProfile));
                    services.AddAutoMapper(typeof(ConfigProfile));
                    services.AddAutoMapper(typeof(DigimonProfile));
                    services.AddAutoMapper(typeof(GameProfile));
                    services.AddAutoMapper(typeof(SecurityProfile));
                    services.AddAutoMapper(typeof(ArenaProfile));
                })
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.SetBasePath(Directory.GetCurrentDirectory());
                    hostConfig.AddEnvironmentVariables("DSO_");
                })
                .Build();

            // Eagerly load static-data bins so a missing/corrupt file fails fast at boot
            // rather than mid-flight when a packet handler first asks for it. CharCreateTable
            // loads first because Digimon_List depends on its enabled-starter set to filter.
            var charCreateTable = host.Services.GetRequiredService<CharCreateTableBinLoader>().Load();
            var dmBase = host.Services.GetRequiredService<DMBaseBinLoader>().Load();

            // Only the 4 client-selectable starters are ever queried at character creation,
            // so the digimon-keyed bins (Digimon_List, DigimonEvo) are filtered to that set
            // at load time. Same hash set is reused for both.
            var starterTypeFilter = charCreateTable.Digimon
                .Where(d => d.Enable)
                .Select(d => d.Model)
                .ToHashSet();
            var digimonList = host.Services.GetRequiredService<DigimonListBinLoader>().Load(starterTypeFilter);
            var digimonEvo = host.Services.GetRequiredService<DigimonEvoBinLoader>().Load(starterTypeFilter);

            var serilog = host.Services.GetRequiredService<ILogger>();
            serilog.Information(
                "Loaded CharCreateTable.bin: {TamerEnabled}/{TamerCount} tamers selectable, {DigimonEnabled}/{DigimonCount} digimon selectable",
                charCreateTable.Tamers.Count(t => t.Enable), charCreateTable.Tamers.Count,
                charCreateTable.Digimon.Count(d => d.Enable), charCreateTable.Digimon.Count);
            serilog.Information(
                "Loaded DMBase.bin (sec 1, level=1 only): {TamerStats} tamer stat rows",
                dmBase.TamerStats.Count);
            serilog.Information(
                "Loaded Digimon_List.bin (filtered to selectable starters): {Count} digimon entries",
                digimonList.ByType.Count);
            serilog.Information(
                "Loaded DigimonEvo.bin (filtered to selectable starters): {Count} evolution trees, {Lines} total evolution lines",
                digimonEvo.ByType.Count, digimonEvo.ByType.Values.Sum(e => e.Lines.Count));

            return host;
        }

        private static ILogger ConfigureLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
                    .WriteTo.RollingFile(configuration["Log:VerboseRepository"] ?? "logs\\Verbose\\CharacterServer", retainedFileCountLimit: 10))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.RollingFile(configuration["Log:DebugRepository"] ?? "logs\\Debug\\CharacterServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.RollingFile(configuration["Log:InformationRepository"] ?? "logs\\Information\\CharacterServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.RollingFile(configuration["Log:WarningRepository"] ?? "logs\\Warning\\CharacterServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.RollingFile(configuration["Log:ErrorRepository"] ?? "logs\\Error\\CharacterServer", retainedFileCountLimit: 5))
                .CreateLogger();
        }
    }
}
