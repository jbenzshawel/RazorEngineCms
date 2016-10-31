namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingPageModelProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "HasParams", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pages", "HasParams");
        }
    }
}
