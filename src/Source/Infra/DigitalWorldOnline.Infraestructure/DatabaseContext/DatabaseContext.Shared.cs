using Microsoft.EntityFrameworkCore;
using DigitalWorldOnline.Commons.DTOs.Base;
using DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared;
using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;

namespace DigitalWorldOnline.Infraestructure
{
    public partial class DatabaseContext
    {
        public DbSet<OwnerItemStorageCharacterReadModel> OwnerItemStorageCharacter { get; set; }
        public DbSet<OwnerItemStorageAccountReadModel> OwnerItemStorageAccount { get; set; }
        public DbSet<OwnerItemStorageCharacterSlotReadModel> OwnerItemStorageCharacterSlots { get; set; }
        public DbSet<OwnerItemStorageAccountSlotReadModel> OwnerItemStorageAccountSlots { get; set; }
        public DbSet<OwnerItemStorageInstanceReadModel> OwnerItemStorageInstances { get; set; }
        public DbSet<OwnerItemStorageInstanceAccessoryStatusReadModel> OwnerItemStorageAccessoryStatuses { get; set; }
        public DbSet<OwnerItemStorageInstanceSocketStatusReadModel> OwnerItemStorageSocketStatuses { get; set; }

        internal static void SharedEntityConfiguration(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new OwnerItemStorageCharacterConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageAccountConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageCharacterSlotConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageAccountSlotConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageInstanceConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageInstanceAccessoryStatusConfiguration());
            builder.ApplyConfiguration(new OwnerItemStorageInstanceSocketStatusConfiguration());
        }
    }
}
