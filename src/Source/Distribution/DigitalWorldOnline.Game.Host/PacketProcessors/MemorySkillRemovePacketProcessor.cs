using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pSkill::RemoveSkillChip</c> (1119) — client requests deletion of a memory
    /// skill from the partner's currently-equipped evolution.  Wire payload is just
    /// <c>u4 SkillCode</c>; we resolve the evolution server-side from
    /// <c>client.Partner.CurrentType</c>, look up the matching memory-skill row, and
    /// remove from DB + in-memory model.  v487 client's
    /// <c>cCliGame::SendDigimonSkillDelete</c> sends only the SkillCode and expects the
    /// server to identify the digimon from session state.
    /// </summary>
    public class MemorySkillRemovePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.MemorySkillRemove;

        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly AssetsLoader _assets;

        public MemorySkillRemovePacketProcessor(ILogger logger, ISender sender, AssetsLoader assets)
        {
            _logger = logger;
            _sender = sender;
            _assets = assets;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);
            int skillCode = packet.ReadInt();

            if (client.Partner == null)
                return;

            var evolution = client.Partner.Evolutions.FirstOrDefault(x => x.Type == client.Partner.CurrentType);
            if (evolution == null)
                return;

            // Validate the skill is actually a memory skill in the bin (the only kind a
            // player should be able to delete).  Inherent slot 0..3 skills cannot be
            // removed — bin's s_nMemorySkill == 0 means "not deletable".
            var skillInfo = _assets.SkillInfo.FirstOrDefault(x => x.SkillId == skillCode);
            if (skillInfo == null || !skillInfo.IsMemorySkill)
            {
                _logger.Warning(
                    "Tamer {TamerId} requested memory-skill delete for SkillId={SkillId} but bin says it's not a memory skill (MemorySkill={MemorySkill}).",
                    client.TamerId, skillCode, skillInfo?.MemorySkill);
                return;
            }

            var owned = evolution.MemorySkills.FirstOrDefault(x => x.SkillId == skillCode);
            if (owned == null)
            {
                _logger.Warning(
                    "Tamer {TamerId} requested delete of memory skill {SkillId} on evolution {EvoType} but they don't own it.",
                    client.TamerId, skillCode, evolution.Type);
                return;
            }

            var removed = await _sender.Send(new RemoveMemorySkillCommand(evolution.Id, skillCode));
            if (removed)
                evolution.MemorySkills.Remove(owned);
        }
    }
}
