namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aka : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.RoleCarriers", newName: "RoleCarrier");
            //RenameTable(name: "dbo.UserRoles", newName: "UserRole");
            //DropForeignKey("dbo.Activities", "Group_Id", "dbo.Groups");
            //DropForeignKey("dbo.Users", "GroupId", "dbo.Groups");
            //DropForeignKey("dbo.Users", "LocationGroupId", "dbo.LocationGroups");
            //DropForeignKey("dbo.RoleActivities", "ActivityId", "dbo.Activities");
            //DropForeignKey("dbo.RoleActivities", "RoleId", "dbo.Roles");
            //DropForeignKey("dbo.LocationGroupLocations", "LocationGroupId", "dbo.Groups");
            //DropForeignKey("dbo.LocationGroupLocations", "LocationId", "dbo.Locations");
            //DropIndex("dbo.Activities", new[] { "Group_Id" });
            //DropIndex("dbo.Users", new[] { "GroupId" });
            //DropIndex("dbo.Users", new[] { "LocationGroupId" });
            //DropIndex("dbo.LocationGroupLocations", new[] { "LocationGroupId" });
            //DropIndex("dbo.LocationGroupLocations", new[] { "LocationId" });
            //DropIndex("dbo.RoleActivities", new[] { "ActivityId" });
            //DropIndex("dbo.RoleActivities", new[] { "RoleId" });
            //DropPrimaryKey("dbo.Groups");
            //CreateTable(
            //    "dbo.SecureItems",
            //    c => new
            //        {
            //            SecureItemId = c.Int(nullable: false, identity: true),
            //            Name = c.String(),
            //        })
            //    .PrimaryKey(t => t.SecureItemId);
            
            //CreateTable(
            //    "dbo.GroupSecureItem",
            //    c => new
            //        {
            //            GroupId = c.Int(nullable: false),
            //            SecureItemId = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.GroupId, t.SecureItemId })
            //    .ForeignKey("dbo.SecureItems", t => t.GroupId, cascadeDelete: true)
            //    .ForeignKey("dbo.Groups", t => t.SecureItemId, cascadeDelete: true)
            //    .Index(t => t.GroupId)
            //    .Index(t => t.SecureItemId);
            
            //CreateTable(
            //    "dbo.GroupUser",
            //    c => new
            //        {
            //            GroupId = c.Int(nullable: false),
            //            UserId = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.GroupId, t.UserId })
            //    .ForeignKey("dbo.Users", t => t.GroupId, cascadeDelete: true)
            //    .ForeignKey("dbo.Groups", t => t.UserId, cascadeDelete: true)
            //    .Index(t => t.GroupId)
            //    .Index(t => t.UserId);
            
            //AddColumn("dbo.Groups", "GroupId", c => c.Int(nullable: false, identity: true));
            //AddColumn("dbo.ReplenOrderDetails", "OrderDetailInfo", c => c.String());
            //AddColumn("dbo.ReplenOrders", "OrderInfo", c => c.String());
            //AddPrimaryKey("dbo.Groups", "GroupId");
            //DropColumn("dbo.Users", "GroupId");
            //DropColumn("dbo.Users", "LocationGroupId");
            //DropColumn("dbo.Groups", "Id");
            //DropTable("dbo.Activities");
            //DropTable("dbo.LocationGroups");
            //DropTable("dbo.LocationGroupLocations");
            //DropTable("dbo.RoleActivities");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RoleActivities",
                c => new
                    {
                        ActivityId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ActivityId, t.RoleId });
            
            CreateTable(
                "dbo.LocationGroupLocations",
                c => new
                    {
                        LocationGroupId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LocationGroupId, t.LocationId });
            
            CreateTable(
                "dbo.LocationGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Sequence = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Activities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Group_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Groups", "Id", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Users", "LocationGroupId", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "GroupId", c => c.Int(nullable: false));
            DropForeignKey("dbo.GroupUser", "UserId", "dbo.Groups");
            DropForeignKey("dbo.GroupUser", "GroupId", "dbo.Users");
            DropForeignKey("dbo.GroupSecureItem", "SecureItemId", "dbo.Groups");
            DropForeignKey("dbo.GroupSecureItem", "GroupId", "dbo.SecureItems");
            DropIndex("dbo.GroupUser", new[] { "UserId" });
            DropIndex("dbo.GroupUser", new[] { "GroupId" });
            DropIndex("dbo.GroupSecureItem", new[] { "SecureItemId" });
            DropIndex("dbo.GroupSecureItem", new[] { "GroupId" });
            DropPrimaryKey("dbo.Groups");
            DropColumn("dbo.ReplenOrders", "OrderInfo");
            DropColumn("dbo.ReplenOrderDetails", "OrderDetailInfo");
            DropColumn("dbo.Groups", "GroupId");
            DropTable("dbo.GroupUser");
            DropTable("dbo.GroupSecureItem");
            DropTable("dbo.SecureItems");
            AddPrimaryKey("dbo.Groups", "Id");
            CreateIndex("dbo.RoleActivities", "RoleId");
            CreateIndex("dbo.RoleActivities", "ActivityId");
            CreateIndex("dbo.LocationGroupLocations", "LocationId");
            CreateIndex("dbo.LocationGroupLocations", "LocationGroupId");
            CreateIndex("dbo.Users", "LocationGroupId");
            CreateIndex("dbo.Users", "GroupId");
            CreateIndex("dbo.Activities", "Group_Id");
            AddForeignKey("dbo.LocationGroupLocations", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocationGroupLocations", "LocationGroupId", "dbo.Groups", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RoleActivities", "RoleId", "dbo.Roles", "RoleId", cascadeDelete: true);
            AddForeignKey("dbo.RoleActivities", "ActivityId", "dbo.Activities", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Users", "LocationGroupId", "dbo.LocationGroups", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Users", "GroupId", "dbo.Groups", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Activities", "Group_Id", "dbo.Groups", "Id");
            RenameTable(name: "dbo.UserRole", newName: "UserRoles");
            RenameTable(name: "dbo.RoleCarrier", newName: "RoleCarriers");
        }
    }
}
