using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class MapRegionAssetConfiguration : IEntityTypeConfiguration<MapRegionAssetDTO>
    {
        public void Configure(EntityTypeBuilder<MapRegionAssetDTO> builder)
        {
            builder
                .ToTable("Asset_MapRegion")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.Index)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
                .Property(e => e.X)
                .HasColumnType("int")
                .HasDefaultValue(6500)
                .IsRequired();
            
            builder
                .Property(e => e.Y)
                .HasColumnType("int")
                .HasDefaultValue(6500)
                .IsRequired();
            
            builder
                .Property(e => e.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}