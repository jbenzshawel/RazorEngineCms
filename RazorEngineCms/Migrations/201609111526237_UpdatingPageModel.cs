namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPageModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "Template", c => c.String());
            AddColumn("dbo.Pages", "CompiledModel", c => c.String());
            AddColumn("dbo.Pages", "CompiledTemplate", c => c.String());
            DropColumn("dbo.Pages", "Content");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Pages", "Content", c => c.String());
            DropColumn("dbo.Pages", "CompiledTemplate");
            DropColumn("dbo.Pages", "CompiledModel");
            DropColumn("dbo.Pages", "Template");
        }
    }
}
