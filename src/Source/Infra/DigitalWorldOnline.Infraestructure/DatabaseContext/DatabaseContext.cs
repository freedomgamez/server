using DigitalWorldOnline.Infraestructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DigitalWorldOnline.Infraestructure
{
    public partial class DatabaseContext : DbContext
    {
        private const string DesignTimeEnvVar = "DSO_DB_CONNECTION";

        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            var cs = Environment.GetEnvironmentVariable(DesignTimeEnvVar)
                ?? throw new InvalidOperationException(
                    $"No DB connection string available at design time. Set environment variable '{DesignTimeEnvVar}'.");

            optionsBuilder.UseDsoMySql(cs);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SharedEntityConfiguration(modelBuilder);
            AccountEntityConfiguration(modelBuilder);
            AssetsEntityConfiguration(modelBuilder);
            CharacterEntityConfiguration(modelBuilder);
            ConfigEntityConfiguration(modelBuilder);
            DigimonEntityConfiguration(modelBuilder);
            EventEntityConfiguration(modelBuilder);
            SecurityEntityConfiguration(modelBuilder);
            ShopEntityConfiguration(modelBuilder);
            MechanicsEntityConfiguration(modelBuilder);
            RoutineEntityConfiguration(modelBuilder);
            ArenaEntityConfiguration(modelBuilder);
        }
    }
}
