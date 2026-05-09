using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.DTOs.Digimon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Digimon
{
    public class DigimonConfiguration : IEntityTypeConfiguration<DigimonDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonDTO> builder)
        {
            builder
                .ToTable("Digimon_Digimon")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.BaseType)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Model)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Level)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(1)
                .IsRequired();

            builder
                .Property(x => x.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(25)
                .IsRequired();

            builder
                .Property(x => x.Size)
                .HasColumnType("smallint")
                .IsRequired();

            builder
                .Property(x => x.CurrentExperience)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.CurrentSkillExperience)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.TranscendenceExperience)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.CreateDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.HatchGrade)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<DigimonHatchGradeEnum, int>(
                    x => (int)x,
                    x => (DigimonHatchGradeEnum)x))
                .IsRequired();

            builder
                .Property(x => x.CurrentType)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Friendship)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(x => x.CurrentHp)
                .HasColumnType("int")
                .HasDefaultValue(150)
                .IsRequired();

            builder
                .Property(x => x.CurrentDs)
                .HasColumnType("int")
                .HasDefaultValue(140)
                .IsRequired();

            builder
                .HasOne(x => x.BuffList)
                .WithOne(x => x.Digimon)
                .HasForeignKey<DigimonBuffListDTO>(x => x.DigimonId);

            builder
                .HasOne(x => x.Location)
                .WithOne(x => x.Digimon)
                .HasForeignKey<DigimonLocationDTO>(x => x.DigimonId);

            builder
               .HasMany(x => x.Evolutions)
               .WithOne(x => x.Digimon)
               .HasForeignKey(x => x.DigimonId);

            builder
                .HasOne(x => x.Digiclone)
                .WithOne(x => x.Digimon)
                .HasForeignKey<DigimonDigicloneDTO>(x => x.DigimonId);

            builder
                .HasOne(x => x.AttributeExperience)
                .WithOne(x => x.Digimon)
                .HasForeignKey<DigimonAttributeExperienceDTO>(x => x.DigimonId);
        }
    }
}