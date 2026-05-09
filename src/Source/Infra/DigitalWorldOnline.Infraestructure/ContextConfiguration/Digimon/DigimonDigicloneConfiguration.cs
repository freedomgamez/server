using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Digimon
{
    public class DigimonDigicloneConfiguration : IEntityTypeConfiguration<DigimonDigicloneDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonDigicloneDTO> builder)
        {
            builder
                .ToTable("Digimon_Digiclone")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.ATLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.ATValue)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.BLLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.BLValue)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.CTLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.CTValue)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.EVLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.EVValue)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.HPLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.HPValue)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .HasOne(x => x.History)
                .WithOne(x => x.DigimonDigiclone)
                .HasForeignKey<DigimonDigicloneHistoryDTO>(x => x.DigimonDigicloneId);
        }
    }
}