using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Digimon
{
    public class DigimonMemorySkillConfiguration : IEntityTypeConfiguration<DigimonMemorySkillDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonMemorySkillDTO> builder)
        {
            builder
                .ToTable("Digimon_EvolutionMemorySkill")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.SkillId)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.CurrentLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue((byte)1)
                .IsRequired();

            builder
                .Property(x => x.MaxLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue((byte)10)
                .IsRequired();

            builder
                .Property(x => x.AcquiredAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.CooldownEndsAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("'1970-01-01 00:00:00.000000'")
                .IsRequired();

            builder
                .HasIndex(x => new { x.EvolutionId, x.SkillId })
                .IsUnique();
        }
    }
}
