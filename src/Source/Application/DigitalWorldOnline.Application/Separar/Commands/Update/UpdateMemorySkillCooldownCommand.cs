using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public sealed class UpdateMemorySkillCooldownCommand : IRequest<Unit>
    {
        public long EvolutionId { get; }
        public int SkillId { get; }
        public DateTime CooldownEndsAt { get; }

        public UpdateMemorySkillCooldownCommand(long evolutionId, int skillId, DateTime cooldownEndsAt)
        {
            EvolutionId = evolutionId;
            SkillId = skillId;
            CooldownEndsAt = cooldownEndsAt;
        }
    }
}
