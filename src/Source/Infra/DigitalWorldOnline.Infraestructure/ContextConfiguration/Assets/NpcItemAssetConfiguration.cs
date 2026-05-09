using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class NpcItemAssetConfiguration : IEntityTypeConfiguration<NpcItemAssetDTO>
    {
        public void Configure(EntityTypeBuilder<NpcItemAssetDTO> builder)
        {
            builder
                .ToTable("Asset_NpcItem")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.ItemId)
                .HasColumnType("int")
                .IsRequired();
        }
    }
}