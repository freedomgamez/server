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
    public sealed partial class PvpServer
    {
        private readonly StatusManager _statusManager;
        private readonly AssetsLoader _assets;
        private readonly ConfigsLoader _configs;
        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly IMapper _mapper;
        private readonly MapRegistry _registry;
        private readonly PvpMapDriver _driver;

        public List<MapInstance> Maps { get; set; }

        public PvpServer(
            AssetsLoader assets,
            ConfigsLoader configs,
            StatusManager statusManager,
            ILogger logger,
            ISender sender,
            IMapper mapper,
            MapRegistry registry,
            PvpMapDriver driver)
        {
            _statusManager = statusManager;
            _assets = assets.Load();
            _configs = configs.Load();
            _logger = logger;
            _sender = sender;
            _mapper = mapper;
            _registry = registry;
            _driver = driver;

            Maps = _registry.GetFlatBacking(MapTypeEnum.Pvp);
        }
    }
}