namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingConstructor : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Pages", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Pages", "Template", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Pages", "Template", c => c.String());
            AlterColumn("dbo.Pages", "Name", c => c.String());
        }
    }
}
