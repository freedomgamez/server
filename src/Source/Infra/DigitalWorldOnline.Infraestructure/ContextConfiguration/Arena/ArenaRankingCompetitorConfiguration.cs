
using DigitalWorldOnline.Commons.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Arena
{
    public class ArenaRankingCompetitorConfiguration : IEntityTypeConfiguration<ArenaRankingCompetitorDTO>
    {
        public void Configure(EntityTypeBuilder<ArenaRankingCompetitorDTO> builder)
        {
            builder
                .ToTable("Arena_Competitor")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.TamerId)
                .HasColumnType("bigint")
                .HasDefaultValueSql("0")
                .IsRequired();

            builder
             .Property(x => x.InsertDate)
             .HasColumnType("datetime(6)")
             .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
             .IsRequired();

            builder
              .Property(x => x.Points)
              .HasColumnType("int")
              .HasDefaultValueSql("0")
              .IsRequired();

            builder
              .Property(x => x.Position)
              .HasColumnType("tinyint unsigned")
              .HasDefaultValueSql("0")
              .IsRequired();

            builder
             .Property(x => x.New)
             .HasColumnType("tinyint(1)")
             .HasDefaultValueSql("0")
             .IsRequired();
     

        }
    }
}