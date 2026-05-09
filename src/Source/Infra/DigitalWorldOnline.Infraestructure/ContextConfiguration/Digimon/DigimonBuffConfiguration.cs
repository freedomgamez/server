using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Digimon
{
    public class DigimonBuffConfiguration : IEntityTypeConfiguration<DigimonBuffDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonBuffDTO> builder)
        {
            builder
                .ToTable("Digimon_Buff")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.BuffId)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.Duration)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.Cooldown)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
               .Property(x => x.TypeN)
               .HasColumnType("int")
               .HasDefaultValue(0)
               .IsRequired();

            builder
                .Property(x => x.SkillId)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.EndDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.CoolEndDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();
        }
    }
}