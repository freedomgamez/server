using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.GameAssets.Mapping;
using DigitalWorldOnline.Application.Admin.Repositories;
using DigitalWorldOnline.Application.Extensions;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.Services;
using DigitalWorldOnline.Commons.DTOs.Shop;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Repositories.Admin;
using DigitalWorldOnline.Game.Managers;
using DigitalWorldOnline.Game.Services;
using DigitalWorldOnline.GameHost;
using DigitalWorldOnline.GameHost.EventsServer;
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

namespace DigitalWorldOnline.Game
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

                    services.AddSingleton<DropManager>();
                    services.AddSingleton<StatusManager>();
                    services.AddSingleton<ExpManager>();
                    services.AddSingleton<PartyManager>();
                    services.AddSingleton<FatigueService>();   // FATIGUE_HOOK
                    services.AddSingleton<GuildLevelService>();
                    services.AddSingleton<HotTimeService>();
                    services.AddSingleton<DailyEventService>();
                    services.AddSingleton<AttendanceService>();
                    services.AddSingleton<OwnerStorageFlushService>();

                    services.AddSingleton<EventQueueManager>();
                    
                    // MapRegistry is the single source of truth for live MapInstance
                    // instances across all four map-owning servers.  Must register
                    // before the servers that inject it (Map Rework Phase A).
                    services.AddSingleton<MapRegistry>();
                    // Per-type lifecycle drivers (Map Rework Phase D).  Each
                    // *Server injects its corresponding driver; the driver owns
                    // CleanIdle + RefreshInstances strategy for that map type.
                    services.AddSingleton<DefaultMapDriver>();
                    services.AddSingleton<DungeonMapDriver>();
                    services.AddSingleton<PvpMapDriver>();
                    services.AddSingleton<EventMapDriver>();
                    services.AddSingleton<MapServer>();
                    services.AddSingleton<PvpServer>();
                    //services.AddSingleton<EventServer>();
                    services.AddSingleton<DungeonsServer>();
                    services.AddSingleton<AssetsLoader>();
                    services.AddSingleton<ConfigsLoader>();
                    services.AddSingleton<GameMasterCommandsProcessor>();

                    // Static-data bin loaders (Phase 1 — eagerly loaded below after Build()).
                    // Each replaces a former DB-backed query handler with an in-memory lookup.
                    services.AddSingleton<DMBaseBinLoader>();
                    services.AddSingleton<DigimonListBinLoader>();
                    services.AddSingleton<DigimonEvoBinLoader>();
                    services.AddSingleton<BuffBinLoader>();
                    services.AddSingleton<AchieveBinLoader>();
                    services.AddSingleton<EventTableBinLoader>();
                    services.AddSingleton<CashShopBinLoader>();
                    services.AddSingleton<SkillBinLoader>();
                    services.AddSingleton<NatureBinLoader>();
                    services.AddSingleton<MonsterBinLoader>();
                    services.AddSingleton<MapBinLoader>();
                    services.AddSingleton<ItemListBinLoader>();

                    services.AddSingleton<ISender, ScopedSender<Mediator>>();
                    services.AddSingleton<IProcessor, GamePacketProcessor>();
                    services.AddSingleton(ConfigureLogger(context.Configuration));

                    services.AddHostedService<GameServer>();

                    services.AddMediatR(
                        typeof(MediatorApplicationHandlerExtension).GetTypeInfo().Assembly,
                        typeof(GameAssetsMarker).GetTypeInfo().Assembly);
                    services.AddScoped<IRequestHandler<ConsignedShopsQuery, IList<ConsignedShopDTO>>, ConsignedShopsQueryHandler>();
                    services.AddTransient<Mediator>();

                    AddAutoMapper(services);
                    AddProcessors(services);
                })
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.SetBasePath(Directory.GetCurrentDirectory());
                    hostConfig.AddEnvironmentVariables("DSO_");
                })
                .Build();

            // Phase 1: eagerly load static-data bins so a missing/corrupt file fails fast at
            // boot rather than mid-flight when the AssetsLoader first asks for it. Each loader
            // satisfies one or more former DB-backed asset queries.
            var dmBase = host.Services.GetRequiredService<DMBaseBinLoader>().Load();
            var digimonList = host.Services.GetRequiredService<DigimonListBinLoader>().Load();
            var digimonEvo = host.Services.GetRequiredService<DigimonEvoBinLoader>().Load();
            var buff = host.Services.GetRequiredService<BuffBinLoader>().Load();
            var achieve = host.Services.GetRequiredService<AchieveBinLoader>().Load();
            var eventTable = host.Services.GetRequiredService<EventTableBinLoader>().Load();
            var cashShop = host.Services.GetRequiredService<CashShopBinLoader>().Load();
            var skill = host.Services.GetRequiredService<SkillBinLoader>().Load();
            var nature = host.Services.GetRequiredService<NatureBinLoader>().Load();
            var monster = host.Services.GetRequiredService<MonsterBinLoader>().Load();
            var mapBin = host.Services.GetRequiredService<MapBinLoader>().Load();
            var itemList = host.Services.GetRequiredService<ItemListBinLoader>().Load();
            // Element-vs-element + attribute-vs-attribute combat multipliers come from
            // Nature.bin — accessed via Utils.GetElementDelta / GetAttributePoint, which
            // drive the boolean HasElementAdvantage / HasAttributeAdvantage extension
            // methods used by every combat call site.
            //
            // NOTE: New_Element.bin (in Pack03) is NOT a combat matrix — it's UI-only
            // data (CsAttributeTypeUI / CsNatureTypeUI: icon paths + screen coordinates)
            // loaded by client DigimonMng, not NatureMng.  No server consumer.
            DigitalWorldOnline.Commons.Utils.UtilitiesFunctions.RegisterNatureSource(nature);
            DigitalWorldOnline.Commons.Utils.UtilitiesFunctions.RegisterMonsterHitFloor(monster.HitByLevel);

            // DMBase.bin section 7 MaxShareStash drives the AccountWarehouse default size
            // so the server matches what v487 client expects (Warehouse.cpp:81 reads s_nMaxShareStash).
            DigitalWorldOnline.Commons.Models.Base.ItemListModel.BinDrivenDefaults[
                DigitalWorldOnline.Commons.Enums.ItemListEnum.AccountWarehouse] = (byte)dmBase.Limit.MaxShareStash;
            var serilog = host.Services.GetRequiredService<ILogger>();
            serilog.Information(
                "Loaded DMBase.bin (all 11 sections): {TamerStats} tamer-level + {DigimonStats} digimon-rank-level + " +
                "{Maps} map-config + {JumpItems} jump-items + {GuildLevels} guild-levels + " +
                "{Stores} person-store-objs + {Penalties} play-penalty + {EvoStages} evo-stage-apply + " +
                "{EvoMax} digimon-evo-max + {DskillOpen} dskill-open-expansion (party/limit single structs)",
                dmBase.TamerStats.Count, dmBase.DigimonStats.Count,
                dmBase.MapInfo.Count, dmBase.JumpBusterDestinations.Count, dmBase.GuildLevels.Count,
                dmBase.PersonStore.Objects.Count, dmBase.PlayPenalty.Count, dmBase.EvolutionStageApply.Count,
                dmBase.DigimonEvoMaxLevel.Count, dmBase.DskillOpenExpansion.Count);
            serilog.Information(
                "Loaded Digimon_List.bin: {Count} digimon entries", digimonList.ByType.Count);
            serilog.Information(
                "Loaded DigimonEvo.bin: {Count} evolution trees, {Lines} total evolution lines",
                digimonEvo.ByType.Count, digimonEvo.ByType.Values.Sum(e => e.Lines.Count));
            serilog.Information(
                "Loaded Buff.bin: {Count} buffs", buff.ById.Count);
            serilog.Information(
                "Loaded Achieve.bin: {Total} achievements ({Titles} grant titles)",
                achieve.All.Count, achieve.All.Count(a => a.BuffCode > 0));
            serilog.Information(
                "Loaded Event.bin: {Daily} daily-play + {Recommend} recommend + {Monthly} monthly + " +
                "{HotTime} hot-time + {DailyCheck} daily-check (attendance window {AttStart:yyyy-MM-dd}..{AttEnd:yyyy-MM-dd})",
                eventTable.Daily.Count, eventTable.Recommend.Count, eventTable.Monthly.Count,
                eventTable.HotTime.Count, eventTable.DailyCheck.Sum(g => g.Rewards.Count),
                eventTable.Attendance.Start, eventTable.Attendance.End);
            serilog.Information(
                "Loaded CashShop.bin: {Total} default-table products ({Active} active)",
                cashShop.ByProductId.Count, cashShop.ByProductId.Values.Count(p => p.Active));
            serilog.Information(
                "Loaded Skill.bin: {Skills} CsSkill + {TamerSkills} CsTamerSkill + {Areas} CsAreaCheck rows",
                skill.ById.Count, skill.TamerSkillsByIndex.Count, skill.AreaChecksByIndex.Count);
            serilog.Information(
                "Loaded Monster.bin: {Mobs} CsMonster + {Hits} level/hit + {Skills} CsMonsterSkill + {Terms} CsMonsterSkillTerms",
                monster.ByType.Count, monster.HitByLevel.Count, monster.SkillsByIndex.Count, monster.TermsByIndex.Count);
            serilog.Information(
                "Loaded map bins: MapList={Maps}, MapPortal={PortalMaps}, MapRegion={RegionMaps}, MapMonsterList={MonsterMaps}",
                mapBin.MapsById.Count, mapBin.PortalsBySourceMapId.Count, mapBin.RegionsByMapId.Count, mapBin.MonstersByMapId.Count);
            serilog.Information(
                "Loaded ItemList.bin static sections: Items={Items}, ItemTap={ItemTap}, CoolTime={CoolTime}, MapDisp={MapDisp}, MapTypeName={MapTypeName}, Rank={Rank}, Element1={Element1}, Element2={Element2}, Exchange={Exchange}, AccessoryOption={AccessoryOption}, AccessoryEnchant={AccessoryEnchant}",
                itemList.Sections.Items,
                itemList.Sections.ItemTap,
                itemList.Sections.CoolTime,
                itemList.Sections.MapDisp,
                itemList.Sections.MapTypeName,
                itemList.Sections.Rank,
                itemList.Sections.ElementItem1,
                itemList.Sections.ElementItem2,
                itemList.Sections.Exchange,
                itemList.Sections.AccessoryOption,
                itemList.Sections.AccessoryEnchant);

            return host;
        }

        private static void AddAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AccountProfile));
            services.AddAutoMapper(typeof(GameAssetsProfile));
            services.AddAutoMapper(typeof(CharacterProfile));
            services.AddAutoMapper(typeof(ConfigProfile));
            services.AddAutoMapper(typeof(DigimonProfile));
            services.AddAutoMapper(typeof(GameProfile));
            services.AddAutoMapper(typeof(SecurityProfile));
            services.AddAutoMapper(typeof(ArenaProfile));
        }

        private static void AddProcessors(IServiceCollection services)
        {
            var packetProcessors = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IGamePacketProcessor).IsAssignableFrom(t) && !t.IsInterface)
                .ToList();

            packetProcessors.ForEach(processor =>
            {
                services.AddSingleton(typeof(IGamePacketProcessor), processor);
            });
        }

        private static ILogger ConfigureLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
                    .WriteTo.RollingFile(configuration["Log:VerboseRepository"] ?? "logs\\Verbose\\GameServer", retainedFileCountLimit: 10))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.RollingFile(configuration["Log:DebugRepository"] ?? "logs\\Debug\\GameServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.RollingFile(configuration["Log:InformationRepository"] ?? "logs\\Information\\GameServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.RollingFile(configuration["Log:WarningRepository"] ?? "logs\\Warning\\GameServer", retainedFileCountLimit: 5))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.RollingFile(configuration["Log:ErrorRepository"] ?? "logs\\Error\\GameServer", retainedFileCountLimit: 5))
                .CreateLogger();
        }
    }
}
