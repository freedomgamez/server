using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    /// <summary>
    /// Attach a memory skill to a digimon evolution.  Returns the new row's Id, or 0
    /// if the (EvolutionId, SkillId) pair already exists.  Caller is responsible for
    /// upstream validation (cap, duplicate, slot-type) before sending.
    /// </summary>
    public sealed class AddMemorySkillCommand : IRequest<long>
    {
        public long EvolutionId { get; }
        public int SkillId { get; }
        public byte MaxLevel { get; }

        public AddMemorySkillCommand(long evolutionId, int skillId, byte maxLevel)
        {
            EvolutionId = evolutionId;
            SkillId = skillId;
            MaxLevel = maxLevel;
        }
    }
}
