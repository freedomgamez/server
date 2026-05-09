using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class ExtraEvolutionNpcAssetConfiguration : IEntityTypeConfiguration<ExtraEvolutionNpcAssetDTO>
    {
        public void Configure(EntityTypeBuilder<ExtraEvolutionNpcAssetDTO> builder)
        {
            builder
           .ToTable("Asset_ExtraEvolutionNpc")
           .HasKey(x => x.Id);

            builder
               .Property(e => e.NpcId)
               .HasColumnType("int")
               .IsRequired();

            builder
               .HasMany(x => x.ExtraEvolutionInformation)
               .WithOne(x => x.ExtraNpcAsset)
               .HasForeignKey(x => x.ExtraNpcId);
        }
    }
}
