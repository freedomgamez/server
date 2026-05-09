using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Models.Asset;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class AchievementAssetConfiguration : IEntityTypeConfiguration<AchievementAssetDTO>
    {
        public void Configure(EntityTypeBuilder<AchievementAssetDTO> builder)
        {
            builder
                .ToTable("Asset_Achievement")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.QuestId)
                .HasColumnType("int")
                .IsRequired();


            builder
                .Property(e => e.Type)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
               .Property(e => e.BuffId)
               .HasColumnType("int")
               .IsRequired();
        }
    }
}