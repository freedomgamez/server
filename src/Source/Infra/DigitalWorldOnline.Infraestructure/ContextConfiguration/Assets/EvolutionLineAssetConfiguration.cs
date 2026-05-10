using DigitalWorldOnline.Commons.DTOs.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Assets
{
    public class EvolutionLineAssetConfiguration : IEntityTypeConfiguration<EvolutionLineAssetDTO>
    {
        public void Configure(EntityTypeBuilder<EvolutionLineAssetDTO> builder)
        {
            builder
                .ToTable("Asset_EvolutionLine")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.Type)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.SlotLevel)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
                .Property(e => e.UnlockLevel)
                .HasColumnType("tinyint unsigned")
                .IsRequired();
            
            builder
                .Property(e => e.UnlockQuestId)
                .HasColumnType("smallint")
                .IsRequired();
            
            builder
                .Property(e => e.UnlockItemSection)
                .HasColumnType("int")
                .IsRequired();
            
            builder
                .Property(e => e.UnlockItemSectionAmount)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.RequiredItem)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.RequiredAmount)
                .HasColumnType("int")
                .IsRequired();

            builder
                .HasMany(x => x.Stages)
                .WithOne(x => x.EvolutionLine)
                .HasForeignKey(x => x.EvolutionLineId);

            // SkillMaxLevels is bin-driven runtime data populated by
            // DigimonEvolutionAssetsQueryHandler from DMBase.bin §12. It belongs to the
            // in-memory DTO/Model only — Asset_EvolutionLine is DB-resident static data
            // staged for retirement once DigimonEvo.bin migration is complete, so adding
            // a new column to it would defeat the whole "static data off MariaDB" plan.
            // EF.Ignore drops it from the schema mapping; the property remains usable
            // on the DTO surface that bin handlers populate.
            builder.Ignore(x => x.SkillMaxLevels);
        }
    }
}