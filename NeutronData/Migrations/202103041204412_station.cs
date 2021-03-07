namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class station : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.OrderDetails", "LineStatusId", "dbo.LineStatus");
            //DropForeignKey("dbo.ReplenOrderDetails", "LineStatusId", "dbo.LineStatus");
            //DropIndex("dbo.OrderDetails", new[] { "LineStatusId" });
            //DropIndex("dbo.ReplenOrderDetails", new[] { "LineStatusId" });
            //CreateTable(
            //    "dbo.LineStatusLookup",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            Name = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //AddColumn("dbo.Languages", "CultureInfo", c => c.String());
            //AlterColumn("dbo.Stations", "Name", c => c.String(nullable: false));
            //DropColumn("dbo.Stations", "TcpConfigurationId");
            //DropColumn("dbo.Stations", "SerialConfigurationId");
            //DropTable("dbo.LineStatus");
            //DropTable("dbo.LocationCounts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LocationCounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InventoryId = c.Int(nullable: false),
                        ItemDefinitionId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        PreviousQty = c.Int(nullable: false),
                        NewQty = c.Int(nullable: false),
                        CountDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LineStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Sequence = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Stations", "SerialConfigurationId", c => c.Int());
            AddColumn("dbo.Stations", "TcpConfigurationId", c => c.Int());
            AlterColumn("dbo.Stations", "Name", c => c.String());
            DropColumn("dbo.Languages", "CultureInfo");
            DropTable("dbo.LineStatusLookup");
            CreateIndex("dbo.ReplenOrderDetails", "LineStatusId");
            CreateIndex("dbo.OrderDetails", "LineStatusId");
            AddForeignKey("dbo.ReplenOrderDetails", "LineStatusId", "dbo.LineStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OrderDetails", "LineStatusId", "dbo.LineStatus", "Id", cascadeDelete: true);
        }
    }
}
