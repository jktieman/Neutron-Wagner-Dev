using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using System.Data.Entity;

namespace NeutronData.DataContexts
{
    public class NeutronDb : DbContext
    {
        public NeutronDb() : base("Neutron")
        {
            Database.SetInitializer(new NullDatabaseInitializer<NeutronDb>());
        }

        public DbSet<OperationStatus> OperationStatuses { get; set; }
        
        public DbSet<ItemDefinition> ItemDefinitions { get; set; }
        public DbSet<StorageType> StorageTypes { get; set; }
        public DbSet<UnitOfIssue> UnitOfIssues { get; set; }
        public DbSet<SizeCode> SizeCodes { get; set; }
        public DbSet<VelocityCode> VelocityCodes { get; set; }
        public DbSet<HeightCode> HeightCodes { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<Location> Locations { get; set; }
       // public DbSet<Station> Stations { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<ShipMethod> ShipMethods { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<ReplenOrder> ReplenOrders { get; set; }
        public DbSet<ReplenOrderDetail> ReplenOrderDetails { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<HardwareDevice> HardwareDevices { get; set; }
        public DbSet<LookupTable> LookupTables { get; set; }
        public DbSet<StorageDeviceType> DeviceTypes { get; set; }
        public DbSet<CommunicationType> CommunicationTypes { get; set; }
        public DbSet<TcpConfiguration> TcpConfigurations { get; set; }
        public DbSet<SerialConfiguration> SerialConfigurations { get; set; }
        public DbSet<PickLocationSkip> PickLocationSkips { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<ActionCodeItem> ActionCodeItems { get; set; }
        public DbSet<LineStatusLookup> LineStatusLookup { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<StationType> StationTypes { get; set; }
        public DbSet<LocationView> LocationViews { get; set; }
        public DbSet<AkaType> AkaTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<Workstation> Workstations { get; set; }
        public DbSet<WorkstationArea> WorkstationAreas { get; set; }
        public DbSet<RFID> RFIDs { get; set; }
        public DbSet<StorageDevice> StorageDevices { get; set; }

        public DbSet<PrintJob> PrintJobs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Groups)
            //    .WithMany(r => r.Users)
            //    .Map(m => m.ToTable("GroupUser")
            //    .MapLeftKey("GroupId")
            //    .MapRightKey("UserId"));


            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Roles)
            //    .WithMany(r => r.Users)
            //    .Map(m => m.ToTable("RoleUser")
            //    .MapLeftKey("RoleId")
            //    .MapRightKey("UserId"));

            //modelBuilder.Entity<Carrier>().
            //    HasMany(u => u.Roles).
            //    WithMany(r => r.Carriers)
            //    .Map(m => m.ToTable("RoleCarrier")
            //    .MapLeftKey("RoleId")
            //    .MapRightKey("CarrierId"));

            //modelBuilder.Entity<SecureItem>().
            //   HasMany(u => u.Groups).
            //   WithMany(r => r.SecureItems)
            //   .Map(m => m.ToTable("GroupSecureItem")
            //   .MapLeftKey("GroupId")
            //   .MapRightKey("SecureItemId"));

            //modelBuilder.Entity<Carrier>().
            //   HasMany(u => u.Roles).
            //   WithMany(r => r.Carriers)
            //   .Map(m => m.ToTable("RoleCarrier")
            //   .MapLeftKey("RoleId")
            //   .MapRightKey("CarrierId"));



            base.OnModelCreating(modelBuilder);

        }
    }
}
