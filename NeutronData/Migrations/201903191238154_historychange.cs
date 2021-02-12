namespace NeutronData.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class historychange : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.History", "TypeCode", c => c.String());
            //AddColumn("dbo.History", "PrimeBin", c => c.String());
            //AddColumn("dbo.History", "NewBin", c => c.String());
            //AddColumn("dbo.History", "TroubleBit", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.History", "TroubleBit");
            DropColumn("dbo.History", "NewBin");
            DropColumn("dbo.History", "PrimeBin");
            DropColumn("dbo.History", "TypeCode");
        }
    }
}
