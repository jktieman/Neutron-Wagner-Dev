namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aka2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Akas",
                c => new
                    {
                        AltAka = c.String(nullable: false, maxLength: 50),
                        Item = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.AltAka);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Akas");
        }
    }
}
