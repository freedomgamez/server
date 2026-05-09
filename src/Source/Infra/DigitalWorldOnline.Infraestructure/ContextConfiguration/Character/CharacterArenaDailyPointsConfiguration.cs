
using DigitalWorldOnline.Commons.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Arena
{
    public class CharacterArenaDailyPointsConfiguration : IEntityTypeConfiguration<CharacterArenaDailyPointsDTO>
    {
        public void Configure(EntityTypeBuilder<CharacterArenaDailyPointsDTO> builder)
        {
            builder
                .ToTable("Character_DailyPoints")
                .HasKey(x => x.Id);


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

        }
    }
}