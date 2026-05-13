using AutoMapper;
using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Map;
using DigitalWorldOnline.Game.Managers;
using DigitalWorldOnline.Commons.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DigitalWorldOnline.GameHost
{
    public sealed partial class MapServer
    {
        private readonly PartyManager _partyManager;
        private readonly StatusManager _statusManager;
        private readonly ExpManager _expManager;
        private readonly DropManager _dropManager;
        private readonly FatigueService _fatigueService;   // FATIGUE_HOOK
        private readonly DailyEventService _dailyEvent;    // C7
        private readonly AssetsLoader _assets;
        private readonly ConfigsLoader _configs;
        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MapRegistry _registry;
        private readonly DefaultMapDriver _driver;

        /// <summary>
        /// Backwards-compatibility view of <see cref="MapRegistry.GetFlatBacking"/>
        /// scoped to <see cref="MapTypeEnum.Default"/>.  Phase B will migrate
        /// callers off this property onto registry lookups directly; for now it
        /// stays so the existing 77 sites keep compiling.
        /// </summary>
        public List<MapInstance> Maps { get; set; }

        public MapServer(
            PartyManager partyManager,
            AssetsLoader assets,
            ConfigsLoader configs,
            StatusManager statusManager,
            ExpManager expManager,
            DropManager dropManager,
            FatigueService fatigueService,   // FATIGUE_HOOK
            DailyEventService dailyEvent,    // C7
            ILogger logger,
            ISender sender,
            IMapper mapper,
            IServiceScopeFactory scopeFactory,
            MapRegistry registry,
            DefaultMapDriver driver)
        {
            _partyManager = partyManager;
            _statusManager = statusManager;
            _expManager = expManager;
            _dropManager = dropManager;
            _fatigueService = fatigueService;   // FATIGUE_HOOK
            _dailyEvent = dailyEvent;          // C7
            _assets = assets.Load();
            _configs = configs.Load();
            _logger = logger;
            _sender = sender;
            _mapper = mapper;
            _scopeFactory = scopeFactory;
            _registry = registry;
            _driver = driver;

            // Share the registry's flat backing list so mutations through
            // _registry.Register / Unregister are visible via Maps and vice-versa.
            Maps = _registry.GetFlatBacking(MapTypeEnum.Default);
        }
    }
}
