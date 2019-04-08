namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aka3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AkaTypes",
                c => new
                    {
                        Aka = c.String(nullable: false, maxLength: 50),
                        Item = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Aka);
            
            DropTable("dbo.Akas");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Akas",
                c => new
                    {
                        AltAka = c.String(nullable: false, maxLength: 50),
                        Item = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.AltAka);
            
            DropTable("dbo.AkaTypes");
        }
    }
}
