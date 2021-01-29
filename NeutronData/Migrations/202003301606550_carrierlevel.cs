namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class carrierlevel : DbMigration
    {
        public override void Up()
        {
           //DopForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes");
           //DropForeignKey("dbo.HardwareDevices", "CommunicationTypeId", "dbo.CommunicationTypes");
           // DropPrimaryKey("dbo.ActionCodeItems");
           // DropPrimaryKey("dbo.CommunicationTypes");
            //AddColumn("dbo.HardwareDevices", "CarrierLevel", c => c.Int(nullable: false));
            //AlterColumn("dbo.ActionCodeItems", "Id", c => c.Int(nullable: false));
           // AlterColumn("dbo.CommunicationTypes", "Id", c => c.Int(nullable: false));
            //AlterColumn("dbo.History", "OrderId", c => c.Int());
            //AlterColumn("dbo.History", "OrderDetailId", c => c.Int());
            //AddPrimaryKey("dbo.ActionCodeItems", "Id");
            //AddPrimaryKey("dbo.CommunicationTypes", "Id");
            //AddForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes", "Id");
            AddForeignKey("dbo.HardwareDevices", "CommunicationTypeId", "dbo.CommunicationTypes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HardwareDevices", "CommunicationTypeId", "dbo.CommunicationTypes");
            DropForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes");
            DropPrimaryKey("dbo.CommunicationTypes");
            DropPrimaryKey("dbo.ActionCodeItems");
            AlterColumn("dbo.History", "OrderDetailId", c => c.Int(nullable: false));
            AlterColumn("dbo.History", "OrderId", c => c.Int(nullable: false));
            AlterColumn("dbo.CommunicationTypes", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ActionCodeItems", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.HardwareDevices", "CarrierLevel");
            AddPrimaryKey("dbo.CommunicationTypes", "Id");
            AddPrimaryKey("dbo.ActionCodeItems", "Id");
            AddForeignKey("dbo.HardwareDevices", "CommunicationTypeId", "dbo.CommunicationTypes", "Id");
            AddForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes", "Id");
        }
    }
}
