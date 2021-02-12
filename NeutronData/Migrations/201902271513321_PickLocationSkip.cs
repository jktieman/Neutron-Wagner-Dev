namespace NeutronData.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PickLocationSkip : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //        "dbo.PickLocationSkips",
            //        c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            RequestedQuantity = c.Int(nullable: false),
            //            Quantity = c.Int(nullable: false),
            //            PickDate = c.DateTime(nullable: false),
            //            OrderDetailId = c.Int(nullable: false),
            //            Inventory_Id = c.Int(),
            //            User_Id = c.Int(),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Inventory", t => t.Inventory_Id)
            //    .ForeignKey("dbo.Users", t => t.User_Id)
            //    .Index(t => t.Inventory_Id)
            //    .Index(t => t.User_Id);
        }
    }
}
