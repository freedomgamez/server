using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateGuildLevelCommand : IRequest
    {
        public long GuildId { get; }
        public byte Level { get; }
        public int CurrentExperience { get; }

        public UpdateGuildLevelCommand(long guildId, byte level, int currentExperience)
        {
            GuildId = guildId;
            Level = level;
            CurrentExperience = currentExperience;
        }
    }
}
