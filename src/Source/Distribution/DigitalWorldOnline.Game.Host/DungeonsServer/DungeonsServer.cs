using AutoMapper;
using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models.Map;
using DigitalWorldOnline.Game.Managers;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.GameHost
{
    public sealed partial class DungeonsServer
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
        private readonly MapRegistry _registry;
        private readonly DungeonMapDriver _driver;

        public List<MapInstance> Maps { get; set; }

        public DungeonsServer(
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
           MapRegistry registry,
           DungeonMapDriver driver)
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
            _registry = registry;
            _driver = driver;

            Maps = _registry.GetFlatBacking(MapTypeEnum.Dungeon);
        }
    }
}