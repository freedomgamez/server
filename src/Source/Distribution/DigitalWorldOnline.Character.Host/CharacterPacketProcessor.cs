using AutoMapper;
using DigitalWorldOnline.Application.Separar.Commands.Create;
using DigitalWorldOnline.Application.Separar.Commands.Delete;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Application.CharacterAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.Character;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Extensions;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Account;
using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Packets.CharacterServer;
using DigitalWorldOnline.Commons.Utils;
using MediatR;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DigitalWorldOnline.Character
{
    public sealed class CharacterPacketProcessor : IProcessor, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly CharCreateTableBinLoader _charCreateTable;

        private const string GameServerAddress = "GameServer:Address";
        private const string GamerServerPublic = "GameServer:PublicAddress";
        private const string GameServerPort = "GameServer:Port";
        private const int HandshakeDegree = 32321;
        private const int HandshakeStampDegree = 65535;

        public CharacterPacketProcessor(ILogger logger,
            ISender sender,
            IConfiguration configuration,
            IMapper mapper,
            CharCreateTableBinLoader charCreateTable)
        {
            _configuration = configuration;
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
            _charCreateTable = charCreateTable;
        }

        /// <summary>
        /// Process the arrived TCP packet, sent from the game client
        /// </summary>
        /// <param name="client">The game client whos sent the packet</param>
        /// <param name="data">The packet bytes array</param>
        public async Task ProcessPacketAsync(GameClient client, byte[] data)
        {
            var packet = new CharacterPacketReader(data);

            switch (packet.Enum)
            {
                case CharacterServerPacketEnum.Connection:
                    {
                        DebugLog("Reading packet parameters...");
                        var kind = packet.ReadByte();

                        var handshakeTimestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var handshake = (short)(client.Handshake ^ HandshakeDegree);

                        client.Send(new ConnectionPacket(handshake, handshakeTimestamp).Serialize());
                    }
                    break;

                case CharacterServerPacketEnum.KeepConnection:
                    break;

                case CharacterServerPacketEnum.RequestCharacters:
                    {
                        packet.Seek(8);

                        DebugLog("Reading packet parameters...");
                        var accountId = packet.ReadUInt();

                        DebugLog($"Getting account {accountId} character list...");
                        var characters = _mapper.Map<List<CharacterModel>>(
                            await _sender.Send(new CharactersByAccountIdQuery(accountId)));

                        await EnsureRequiredAccountListsAsync(accountId);
                        await EnsureRequiredCharacterListsAsync(characters);

                        //characters.ForEach(character => 
                        //{
                        //    if(character.Partner.CurrentType != character.Partner.BaseType)
                        //    {
                        //        DebugLog($"Updating partner's current type...");
                        //        character.Partner.UpdateCurrentType(character.Partner.BaseType);
                        //        _sender.Send(new UpdatePartnerCurrentTypeCommand(character.Partner));
                        //    }
                        //});

                        client.Send(new CharacterListPacket(characters));

                        client.SetAccountId(accountId);
                    }
                    break;

                case CharacterServerPacketEnum.CreateCharacter:
                    {
                        DebugLog("Reading packet parameters...");
                        var position = packet.ReadByte();
                        var tamerModel = packet.ReadInt();
                        var tamerName = packet.ReadZString();
                        packet.Seek(42);
                        var digimonModel = packet.ReadInt();
                        var digimonName = packet.ReadZString();

                        // Validate against CharCreateTable.bin. The client UI shows entries with
                        // bShow=true but only lets the player actually pick entries with
                        // bEnable=true. In v487 that's 4-of-12 tamers (Marcus/Touma/Yoshi/Ikuto)
                        // and 4-of-86 digimon (Agumon/Lalamon/Gaomon/Falcomon). Anything else
                        // is a desynced client or a tampered packet — refuse.
                        var ccTable = _charCreateTable.Data;
                        var tamerEntry = ccTable.FindTamer(tamerModel);
                        var digimonEntry = ccTable.FindStarterDigimon(digimonModel);
                        if (tamerEntry is not { Enable: true } || digimonEntry is not { Enable: true })
                        {
                            _logger.Warning(
                                "Rejecting CreateCharacter from account {Account}: " +
                                "tamerModel={TamerModel} (known={TamerKnown}, enable={TamerEnable}), " +
                                "digimonModel={DigimonModel} (known={DigimonKnown}, enable={DigimonEnable})",
                                client.AccountId,
                                tamerModel, tamerEntry != null, tamerEntry?.Enable,
                                digimonModel, digimonEntry != null, digimonEntry?.Enable);
                            break;
                        }

                        DebugLog($"Searching account with id {client.AccountId}...");
                        var account = _mapper.Map<AccountModel>(await _sender.Send(new AccountByIdQuery(client.AccountId)));

                        //tamerName = tamerName.ModeratorPrefix(account.AccessLevel);
                        //DebugLog($"{tamerName}");

                        DebugLog("Creating character...");
                        var character = CharacterModel.Create(
                            client.AccountId,
                            tamerName,
                            tamerModel,
                            position,
                            account.LastPlayedServer);

                        DebugLog("Creating digimon...");
                        var digimon = DigimonModel.Create(
                            digimonName,
                            digimonModel,
                            digimonModel,
                            DigimonHatchGradeEnum.Perfect,
                            UtilitiesFunctions.RandomShort(12000, 12501),
                            0);

                        character.AddDigimon(digimon);

                        var handshakeTimestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var handshake = (short)(handshakeTimestamp & HandshakeStampDegree);

                        client.Send(new CharacterCreatedPacket(character, handshake));

                        //TODO: remover busca de status assets daqui
                        DebugLog("Getting tamer status information...");
                        // Per-model tamer "base status" retired — see InitialInformationPacketProcessor.
                        // The TamerLevelStatusQuery below (DMBase.bin §1) carries the full per-level
                        // stat block; equipment/socket/buff add on top in CharacterModelBehavior.
                        //character.SetBaseStatus(
                        //    _mapper.Map<CharacterBaseStatusAssetModel>(
                        //        await _sender.Send(
                        //            new TamerBaseStatusQuery(character.Model)
                        //        )));

                        character.SetLevelStatus(
                            _mapper.Map<CharacterLevelStatusAssetModel>(
                                await _sender.Send(
                                    new TamerLevelStatusQuery(character.Model,
                                    character.Level)
                                )));

                        character.Partner.SetBaseInfo(
                            _mapper.Map<DigimonBaseInfoAssetModel>(
                                await _sender.Send(
                                    new DigimonBaseInfoQuery(character.Partner.CurrentType)
                                )));

                        DebugLog($"Registering tamer and digimon for account {account.Username}...");
                        character.Partner.AddEvolutions(await _sender.Send(new DigimonEvolutionAssetsByTypeQuery(digimonModel)));
                        await _sender.Send(new CreateCharacterCommand(character));
                    }
                    break;

                case CharacterServerPacketEnum.CheckNameDuplicity:
                    {
                        DebugLog("Getting parameters...");
                        var tamerName = packet.ReadString();

                        DebugLog($"{tamerName}");

                        InfoLog("Searching account...");
                        var account = _mapper.Map<AccountModel>(await _sender.Send(new AccountByIdQuery(client.AccountId)));

                        tamerName = tamerName.ModeratorPrefix(account.AccessLevel);
                        DebugLog($"{tamerName}");

                        InfoLog("Checking tamer name duplicity...");
                        var availableName = await _sender.Send(new CharacterByNameQuery(tamerName)) == null;

                        DebugLog($"{availableName}");

                        DebugLog("Sending answer...");
                        client.Send(new AvailableNamePacket(availableName).Serialize());
                    }
                    break;

                case CharacterServerPacketEnum.DeleteCharacter:
                    {
                        DebugLog($"Reading packet parameters...");
                        var position = packet.ReadByte();
                        packet.Skip(3);
                        var validation = packet.ReadString();

                        DebugLog($"Searching account with id {client.AccountId}...");
                        var account = _mapper.Map<AccountModel>(await _sender.Send(new AccountByIdQuery(client.AccountId)));

                        if (account.CharacterDeleteValidation(validation))
                        {
                            DebugLog($"Deleting character...");
                            var deletedCharacter = await _sender.Send(new DeleteCharacterCommand(client.AccountId, position));

                            client.Send(new CharacterDeletedPacket(deletedCharacter).Serialize());
                        }
                        else
                        {
                            DebugLog($"Validation fail for deleting character in account {account.Username}.");

                            client.Send(new CharacterDeletedPacket(DeleteCharacterResultEnum.ValidationFail).Serialize());
                        }
                    }
                    break;

                case CharacterServerPacketEnum.GetCharacterPosition:
                    {
                        DebugLog($"Reading parameters...");
                        var position = packet.ReadByte();

                        DebugLog($"Searching character...");
                        var character = _mapper.Map<CharacterModel>(await _sender.Send(new CharacterByAccountIdAndPositionQuery(client.AccountId, position)));

                        while (character == null)
                        {
                            await Task.Delay(1500);
                            DebugLog($"Searching character again...");
                            character = _mapper.Map<CharacterModel>(await _sender.Send(new CharacterByAccountIdAndPositionQuery(client.AccountId, position)));
                        }

                        DebugLog($"Updating access information for account {client.AccountId}.");
                        await _sender.Send(new UpdateLastPlayedCharacterCommand(client.AccountId, character.Id));

                        DebugLog($"Updating character's channel...");
                        await _sender.Send(new UpdateCharacterChannelCommand(character.Id));

                        DebugLog($"Updating account welcome flag...");
                        await _sender.Send(new UpdateAccountWelcomeFlagCommand(character.AccountId));

                        await EnsureRequiredAccountListsAsync(client.AccountId);
                        await EnsureRequiredCharacterListsAsync(new List<CharacterModel> { character });

                        DebugLog($"Sending selected server info...");
                        client.Send(new ConnectGameServerInfoPacket(
                            _configuration[GameServerAddress],
                            _configuration[GameServerPort],
                            character.Location.MapId).Serialize());
                    }
                    break;

                case CharacterServerPacketEnum.ConnectGameServer:
                    {
                        DebugLog("Sending answer to connect to game server...");
                        client.Send(new ConnectGameServerPacket().Serialize());
                    }
                    break;

                default:
                    _logger.Warning($"Unknown packet. Type: {packet.Type} Length: {packet.Length}.");
                    break;
            }
        }

        private async Task EnsureRequiredCharacterListsAsync(List<CharacterModel> characters)
        {
            var requiredTypes = new[]
            {
                ItemListEnum.Equipment,
                ItemListEnum.Inventory,
                ItemListEnum.Warehouse,
                ItemListEnum.Chipsets,
                ItemListEnum.JogressChipset,
                ItemListEnum.Digivice,
                ItemListEnum.TamerSkill,
                ItemListEnum.RewardWarehouse,
                ItemListEnum.GiftWarehouse,
                ItemListEnum.ConsignedWarehouse,
                ItemListEnum.TamerShop,
                ItemListEnum.ConsignedShop
            };

            foreach (var character in characters)
            {
                var loadedTypes = character.ItemList.Select(x => x.Type).ToHashSet();
                foreach (var type in requiredTypes)
                {
                    if (!loadedTypes.Contains(type))
                    {
                        await _sender.Send(new CreateCharacterItemListCommand(character.Id, type));
                        loadedTypes.Add(type);
                    }
                }
            }

            for (var i = 0; i < characters.Count; i++)
            {
                var refreshed = _mapper.Map<CharacterModel>(await _sender.Send(new CharacterByIdQuery(characters[i].Id)));
                if (refreshed != null)
                    characters[i] = refreshed;
            }
        }

        private async Task EnsureRequiredAccountListsAsync(long accountId)
        {
            var requiredTypes = new[]
            {
                ItemListEnum.AccountWarehouse,
                ItemListEnum.CashWarehouse,
                ItemListEnum.ShopWarehouse,
                ItemListEnum.BuyHistory
            };

            foreach (var type in requiredTypes)
                await _sender.Send(new CreateAccountItemListCommand(accountId, type));
        }

        /// <summary>
        /// Shortcut for debug logging with client and packet info.
        /// </summary>
        /// <param name="message">The message to log</param>
        private void DebugLog(string message)
        {
            _logger?.Debug($"{message}");
        }

        /// <summary>
        /// Shortcut for info logging.
        /// </summary>
        /// <param name="message">The message to log</param>
        private void InfoLog(string message)
        {
            _logger?.Information($"{message}");
        }

        /// <summary>
        /// Disposes the entire object.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
