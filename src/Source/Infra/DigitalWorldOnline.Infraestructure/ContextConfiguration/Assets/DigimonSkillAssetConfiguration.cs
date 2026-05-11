using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class DigimonSkillAssetConfiguration : IEntityTypeConfiguration<DigimonSkillAssetDTO>
    {
        public void Configure(EntityTypeBuilder<DigimonSkillAssetDTO> builder)
        {
            builder
                .ToTable("Asset_DigimonSkill")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.Type)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.Slot)
                .HasColumnType("tinyint unsigned")
                .IsRequired();
            
            builder
                .Property(e => e.SkillId)
                .HasColumnType("int")
                .IsRequired();

            // s_Skill[i].s_nReqPrevSkillLevel from Digimon_List.bin — previous-slot gate.
            builder
                .Property(e => e.RequiredLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue((byte)0)
                .IsRequired();
        }
    }
}