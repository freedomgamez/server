using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class SkillCodeAssetConfiguration : IEntityTypeConfiguration<SkillCodeAssetDTO>
    {
        public void Configure(EntityTypeBuilder<SkillCodeAssetDTO> builder)
        {
            builder
                .ToTable("Asset_SkillCode")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.SkillCode)
                .HasColumnType("bigint")
                .IsRequired();

            builder
                .Property(e => e.Comment)
                .HasColumnType("varchar(255)")
                .HasMaxLength(200)
                .IsRequired();

            builder
                .HasMany(x => x.Apply)
                .WithOne(x => x.SkillCodeAsset)
                .HasForeignKey(x => x.SkillCodeAssetId);
        }
    }
}