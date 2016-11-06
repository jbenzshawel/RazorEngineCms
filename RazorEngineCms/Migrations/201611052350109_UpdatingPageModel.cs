namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPageModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Pages", "Updated", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Pages", "Updated", c => c.DateTime(nullable: false));
        }
    }
}
