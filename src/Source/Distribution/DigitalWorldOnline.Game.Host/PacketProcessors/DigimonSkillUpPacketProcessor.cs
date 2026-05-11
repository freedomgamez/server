using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// SkillLevelUp packet handler — mirrors the client's <c>cSkillWin::_CheckSkillUp</c>
    /// gate sequence from <c>DigimonSkill.cpp:104-145</c>. All four checks (max-level,
    /// digimon-level vs s_nLimitLevel, prereq-slot vs s_nReqPrevSkillLevel, point balance)
    /// must pass before committing the level-up.  Previously only the level increment ran,
    /// so a crafted packet could push any slot past gates the client UI would block.
    /// </summary>
    public class DigimonSkillUpPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.SkillLevelUp;

        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly AssetsLoader _assets;

        public DigimonSkillUpPacketProcessor(
            ILogger logger,
            ISender sender,
            AssetsLoader assets)
        {
            _logger = logger;
            _sender = sender;
            _assets = assets;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            int generalHandler = packet.ReadInt();
            byte formSlot = packet.ReadByte();
            byte skillSlot = packet.ReadByte();

            // formSlot is 1-indexed (client F1..Fn maps to evolution slot 1..n).
            if (formSlot == 0 || formSlot > client.Tamer.Partner.Evolutions.Count)
            {
                _logger.Warning($"SkillUp: bad formSlot {formSlot} (have {client.Tamer.Partner.Evolutions.Count} evolutions) from tamer {client.TamerId}");
                return;
            }

            var evolution = client.Tamer.Partner.Evolutions[formSlot - 1];
            if (evolution == null)
            {
                _logger.Warning($"SkillUp: null evolution at form {formSlot} for tamer {client.TamerId}");
                return;
            }

            if (skillSlot >= evolution.Skills.Count)
            {
                _logger.Warning($"SkillUp: bad skillSlot {skillSlot} (have {evolution.Skills.Count}) for tamer {client.TamerId}");
                return;
            }

            // Resolve the bin-driven skill entry for this digimon-type × slot.
            var skillAsset = _assets.DigimonSkillInfo.FirstOrDefault(
                x => x.Type == client.Partner.CurrentType && x.Slot == skillSlot);
            if (skillAsset == null)
            {
                _logger.Warning($"SkillUp: no skill at slot {skillSlot} for digimon type {client.Partner.CurrentType}");
                return;
            }

            var skillEntry = evolution.Skills[skillSlot];

            // (1) Max level reached — bail before consuming anything.  Mirrors `IncreaseSkillLevel` guard.
            if (skillEntry.CurrentLevel >= skillEntry.MaxLevel)
                return;

            // (2) Digimon level vs skill UnlockLevel (bin's s_nLimitLevel) — `DigimonSkill.cpp:65`.
            if (skillAsset.SkillInfo != null && client.Partner.Level < skillAsset.SkillInfo.UnlockLevel)
            {
                _logger.Warning($"SkillUp: tamer {client.TamerId} partner level {client.Partner.Level} < UnlockLevel {skillAsset.SkillInfo.UnlockLevel} for skill {skillAsset.SkillId}");
                return;
            }

            // (3) Prereq slot gate (bin's s_nReqPrevSkillLevel) — `DigimonSkill.cpp:113-122`.
            if (skillAsset.RequiredLevel > 0)
            {
                if (skillSlot == 0)
                {
                    _logger.Warning($"SkillUp: malformed bin — slot 0 carries RequiredLevel {skillAsset.RequiredLevel} for type {client.Partner.CurrentType}");
                    return;
                }
                var prevLevel = evolution.Skills[skillSlot - 1].CurrentLevel;
                if (prevLevel < skillAsset.RequiredLevel)
                {
                    _logger.Warning($"SkillUp: tamer {client.TamerId} prereq slot {skillSlot - 1} level {prevLevel} < required {skillAsset.RequiredLevel}");
                    return;
                }
            }

            // (4) Point balance — must have enough SP to cover RequiredPoints.
            byte cost = skillAsset.SkillInfo?.RequiredPoints ?? 0;
            if (cost == 0)
            {
                _logger.Warning($"SkillUp: skill {skillAsset.SkillId} has no RequiredPoints (asset not loaded?)");
                return;
            }
            if (evolution.SkillPoints < cost)
                return;

            // All gates pass — commit.
            evolution.DecreaseSkillPoints(cost);
            if (!evolution.Skills[skillSlot].IncreaseSkillLevel())
            {
                _logger.Warning($"SkillUp: IncreaseSkillLevel refused after gate-pass (race?)  tamer {client.TamerId} slot {skillSlot}");
                return;
            }

            _logger.Debug($"SkillUp: tamer {client.TamerId} slot {skillSlot} → level {evolution.Skills[skillSlot].CurrentLevel}; consumed {cost} SP ({evolution.SkillPoints} remaining)");
            await _sender.Send(new UpdateEvolutionCommand(evolution));
        }
    }
}
