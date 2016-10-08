namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPagePropName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "Section", c => c.String());
            DropColumn("dbo.Pages", "Variable");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Pages", "Variable", c => c.String());
            DropColumn("dbo.Pages", "Section");
        }
    }
}
