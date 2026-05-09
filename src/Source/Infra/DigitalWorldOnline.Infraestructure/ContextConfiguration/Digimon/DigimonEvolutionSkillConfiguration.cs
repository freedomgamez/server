using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Digimon
{
    public class DigimonEvolutionSkillConfiguration : IEntityTypeConfiguration<DigimonEvolutionSkillDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonEvolutionSkillDTO> builder)
        {
            builder
                .ToTable("Digimon_EvolutionSkill")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.CurrentLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.EndDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
               .Property(x => x.Duration)
               .HasColumnType("int")
               .HasDefaultValue(0)
               .IsRequired();

            builder
                .Property(x => x.MaxLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(10)
                .IsRequired();


        }
    }
}