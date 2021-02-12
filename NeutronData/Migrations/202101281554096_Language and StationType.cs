namespace NeutronData.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class LanguageandStationType : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes");
            //DropForeignKey("dbo.Users", "HomeLocationId", "dbo.Locations");
            //DropIndex("dbo.Users", new[] { "HomeLocationId" });
            //DropIndex("dbo.Stations", new[] { "CommunicationTypeId" });
            //CreateTable(
            //    "dbo.Languages",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: false),
            //            Name = c.String(),
            //            Sequence = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.StationTypes",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: false),
            //            Name = c.String(),
            //            Sequence = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //AddColumn("dbo.Users", "LanguageId", c => c.Int(nullable: false));
            //AddColumn("dbo.Stations", "StationTypeId", c => c.Int(nullable: false));
           // CreateIndex("dbo.Users", "LanguageId");
           // CreateIndex("dbo.Stations", "StationTypeId");
            //AddForeignKey("dbo.Users", "LanguageId", "dbo.Languages", "Id", cascadeDelete: true);
           // AddForeignKey("dbo.Stations", "StationTypeId", "dbo.StationTypes", "Id", cascadeDelete: true);
            //DropColumn("dbo.Users", "HomeLocationId");
            //DropColumn("dbo.Stations", "CommunicationTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Stations", "CommunicationTypeId", c => c.Int());
            AddColumn("dbo.Users", "HomeLocationId", c => c.Int());
            DropForeignKey("dbo.Stations", "StationTypeId", "dbo.StationTypes");
            DropForeignKey("dbo.Users", "LanguageId", "dbo.Languages");
            DropIndex("dbo.Stations", new[] { "StationTypeId" });
            DropIndex("dbo.Users", new[] { "LanguageId" });
            DropColumn("dbo.Stations", "StationTypeId");
            DropColumn("dbo.Users", "LanguageId");
            DropTable("dbo.StationTypes");
            DropTable("dbo.Languages");
            CreateIndex("dbo.Stations", "CommunicationTypeId");
            CreateIndex("dbo.Users", "HomeLocationId");
            AddForeignKey("dbo.Users", "HomeLocationId", "dbo.Locations", "Id");
            AddForeignKey("dbo.Stations", "CommunicationTypeId", "dbo.CommunicationTypes", "Id");
        }
    }
}
