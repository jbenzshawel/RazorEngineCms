namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPageModelToHaveHasInclude : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "HasInclude", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pages", "HasInclude");
        }
    }
}
