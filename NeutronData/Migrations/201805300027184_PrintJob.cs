namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PrintJob : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrintJobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        JobNum = c.String(),
                        PickDocument = c.Boolean(nullable: false),
                        ToteLabel = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PrintJobs");
        }
    }
}
