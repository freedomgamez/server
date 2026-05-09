using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Arena
{
    public class ArenaRankingConfiguration : IEntityTypeConfiguration<ArenaRankingDTO>
    {
        public void Configure(EntityTypeBuilder<ArenaRankingDTO> builder)
        {
            builder
                .ToTable("Arena_Ranking")
                .HasKey(x => x.Id);

            builder
                  .Property(x => x.Type)
                  .HasColumnType("int")
                  .HasConversion(new ValueConverter<ArenaRankingEnum, int>(
                      x => (int)x,
                      x => (ArenaRankingEnum)x))
                  .IsRequired();

            builder
                .Property(x => x.StartDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.EndDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .HasMany(x => x.Competitors)
                .WithOne(x => x.Ranking)
                .HasForeignKey(x => x.RankingId);
        }
    }
}