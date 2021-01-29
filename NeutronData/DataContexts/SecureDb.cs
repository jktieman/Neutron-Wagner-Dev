using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models.Lookups;

namespace NeutronData.DataContexts
{
    public class SecureDb : DbContext
    {
        public SecureDb() : base("name=Neutron") { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<RoleUser> RoleUser { get; set; }
        public DbSet<RoleCarrier> RoleCarrier { get; set; }
        public DbSet<GroupUser> GroupUser { get; set; }
        public DbSet<GroupSecureItem> GroupSecureItem { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Roles)
            //    .WithMany(r => r.Users)
            //    .Map(m => m.ToTable("RoleUser")
            //    .MapLeftKey("RoleId")
            //    .MapRightKey("UserId"));

            //modelBuilder.Entity<Carrier>()
            //    .HasMany(u => u.Roles)
            //    .WithMany(r => r.Carriers)
            //    .Map(m => m.ToTable("RoleCarrier")
            //    .MapLeftKey("CarrierId")
            //    .MapRightKey("RoleId"));

            //modelBuilder.Entity<SecureItem>()
            //    .HasMany(u => u.Groups)
            //    .WithMany(r => r.SecureItems)
            //    .Map(m => m.ToTable("GroupSecureItem")
            //    .MapLeftKey("GroupId")
            //    .MapRightKey("SecureItemId"));

            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Groups)
            //    .WithMany(r => r.Users)
            //    .Map(m => m.ToTable("GroupUser")
            //    .MapLeftKey("GroupId")
            //    .MapRightKey("UserId"));

            //modelBuilder.Entity<Group>()
            //    .HasMany(u => u.Users)
            //    .WithMany(g => g.Groups)
            //    .Map(m => m.ToTable("GroupUser")
            //    .MapLeftKey("GroupId")
            //    .MapRightKey("UserId"));

            //modelBuilder.Entity<Group>()
            //    .HasMany(u => u.SecureItems)
            //    .WithMany(g => g.Groups)
            //    .Map(m => m.ToTable("GroupSecureItem")
            //    .MapLeftKey("GroupId")
            //    .MapRightKey("SecureItemId"));

            base.OnModelCreating(modelBuilder);
        }
    }
}