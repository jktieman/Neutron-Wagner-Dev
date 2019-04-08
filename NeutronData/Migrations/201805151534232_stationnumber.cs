namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stationnumber : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.RoleCarrier", newName: "RoleCarriers");
            //RenameTable(name: "dbo.UserRole", newName: "UserRoles");
            //RenameTable(name: "dbo.GroupUser", newName: "GroupUsers");
            //RenameTable(name: "dbo.GroupSecureItem", newName: "SecureItemGroups");
            //RenameColumn(table: "dbo.RoleCarriers", name: "CarrierId", newName: "Carrier_CarrierId");
            //RenameColumn(table: "dbo.RoleCarriers", name: "RoleId", newName: "Role_RoleId");
            //RenameColumn(table: "dbo.UserRoles", name: "UserId", newName: "User_Id");
            //RenameColumn(table: "dbo.UserRoles", name: "RoleId", newName: "Role_RoleId");
            //RenameColumn(table: "dbo.GroupUsers", name: "GroupId", newName: "User_Id");
            //RenameColumn(table: "dbo.GroupUsers", name: "UserId", newName: "Group_GroupId");
            //RenameColumn(table: "dbo.SecureItemGroups", name: "GroupId", newName: "SecureItem_SecureItemId");
            //RenameColumn(table: "dbo.SecureItemGroups", name: "SecureItemId", newName: "Group_GroupId");
            //RenameIndex(table: "dbo.RoleCarriers", name: "IX_RoleId", newName: "IX_Role_RoleId");
            //RenameIndex(table: "dbo.RoleCarriers", name: "IX_CarrierId", newName: "IX_Carrier_CarrierId");
            //RenameIndex(table: "dbo.SecureItemGroups", name: "IX_GroupId", newName: "IX_SecureItem_SecureItemId");
            //RenameIndex(table: "dbo.SecureItemGroups", name: "IX_SecureItemId", newName: "IX_Group_GroupId");
            //RenameIndex(table: "dbo.GroupUsers", name: "IX_UserId", newName: "IX_Group_GroupId");
            //RenameIndex(table: "dbo.GroupUsers", name: "IX_GroupId", newName: "IX_User_Id");
            //RenameIndex(table: "dbo.UserRoles", name: "IX_UserId", newName: "IX_User_Id");
            //RenameIndex(table: "dbo.UserRoles", name: "IX_RoleId", newName: "IX_Role_RoleId");
            //DropPrimaryKey("dbo.RoleCarriers");
            //DropPrimaryKey("dbo.GroupUsers");
            //AddColumn("dbo.Carriers", "StationNumber", c => c.Int(nullable: false));
            //AddPrimaryKey("dbo.RoleCarriers", new[] { "Role_RoleId", "Carrier_CarrierId" });
            //AddPrimaryKey("dbo.GroupUsers", new[] { "Group_GroupId", "User_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.GroupUsers");
            DropPrimaryKey("dbo.RoleCarriers");
            DropColumn("dbo.Carriers", "StationNumber");
            AddPrimaryKey("dbo.GroupUsers", new[] { "GroupId", "UserId" });
            AddPrimaryKey("dbo.RoleCarriers", new[] { "CarrierId", "RoleId" });
            RenameIndex(table: "dbo.UserRoles", name: "IX_Role_RoleId", newName: "IX_RoleId");
            RenameIndex(table: "dbo.UserRoles", name: "IX_User_Id", newName: "IX_UserId");
            RenameIndex(table: "dbo.GroupUsers", name: "IX_User_Id", newName: "IX_GroupId");
            RenameIndex(table: "dbo.GroupUsers", name: "IX_Group_GroupId", newName: "IX_UserId");
            RenameIndex(table: "dbo.SecureItemGroups", name: "IX_Group_GroupId", newName: "IX_SecureItemId");
            RenameIndex(table: "dbo.SecureItemGroups", name: "IX_SecureItem_SecureItemId", newName: "IX_GroupId");
            RenameIndex(table: "dbo.RoleCarriers", name: "IX_Carrier_CarrierId", newName: "IX_CarrierId");
            RenameIndex(table: "dbo.RoleCarriers", name: "IX_Role_RoleId", newName: "IX_RoleId");
            RenameColumn(table: "dbo.SecureItemGroups", name: "Group_GroupId", newName: "SecureItemId");
            RenameColumn(table: "dbo.SecureItemGroups", name: "SecureItem_SecureItemId", newName: "GroupId");
            RenameColumn(table: "dbo.GroupUsers", name: "Group_GroupId", newName: "UserId");
            RenameColumn(table: "dbo.GroupUsers", name: "User_Id", newName: "GroupId");
            RenameColumn(table: "dbo.UserRoles", name: "Role_RoleId", newName: "RoleId");
            RenameColumn(table: "dbo.UserRoles", name: "User_Id", newName: "UserId");
            RenameColumn(table: "dbo.RoleCarriers", name: "Role_RoleId", newName: "RoleId");
            RenameColumn(table: "dbo.RoleCarriers", name: "Carrier_CarrierId", newName: "CarrierId");
            RenameTable(name: "dbo.SecureItemGroups", newName: "GroupSecureItem");
            RenameTable(name: "dbo.GroupUsers", newName: "GroupUser");
            RenameTable(name: "dbo.UserRoles", newName: "UserRole");
            RenameTable(name: "dbo.RoleCarriers", newName: "RoleCarrier");
        }
    }
}
