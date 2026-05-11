using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public sealed class RemoveMemorySkillCommand : IRequest<bool>
    {
        public long EvolutionId { get; }
        public int SkillId { get; }

        public RemoveMemorySkillCommand(long evolutionId, int skillId)
        {
            EvolutionId = evolutionId;
            SkillId = skillId;
        }
    }
}
