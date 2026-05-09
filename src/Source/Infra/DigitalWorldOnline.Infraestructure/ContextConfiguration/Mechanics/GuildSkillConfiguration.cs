using DigitalWorldOnline.Commons.DTOs.Mechanics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Mechanics
{
    public class GuildSkillConfiguration : IEntityTypeConfiguration<GuildSkillDTO>
    {
        public void Configure(EntityTypeBuilder<GuildSkillDTO> builder)
        {
            builder
                .ToTable("Guild_Skill")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Level)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
            
            builder
                .Property(x => x.SkillId)
                .HasColumnType("int")
                .IsRequired();
        }
    }
}