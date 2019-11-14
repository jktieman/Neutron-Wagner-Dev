namespace NeutronData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionCodeItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActionCodeItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.Inventory", "ReceivedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Inventory", "ReceivedDate", c => c.DateTime());
            DropTable("dbo.ActionCodeItems");
        }
    }
}
