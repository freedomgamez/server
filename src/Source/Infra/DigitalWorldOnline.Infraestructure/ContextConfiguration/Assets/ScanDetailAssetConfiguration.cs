using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class ScanDetailAssetConfiguration : IEntityTypeConfiguration<ScanDetailAssetDTO>
    {
        public void Configure(EntityTypeBuilder<ScanDetailAssetDTO> builder)
        {
            builder
                .ToTable("Asset_ScanDetail")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.MinAmount)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(e => e.MaxAmount)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(e => e.ItemId)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.ItemName)
                .HasColumnType("varchar(255)")
                .HasMaxLength(100);

            builder
                .HasMany(x => x.Rewards)
                .WithOne(x => x.ScanDetailAsset)
                .HasForeignKey(x => x.ScanDetailAssetId);
        }
    }
}