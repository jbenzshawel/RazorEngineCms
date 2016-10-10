namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingUpdatingProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pages", "Updated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pages", "Updated");
        }
    }
}
