namespace RazorEngineCms.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingIncludeModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Includes", "Updated", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Includes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Includes", "Content", c => c.String(nullable: false));
            AlterColumn("dbo.Includes", "Type", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Includes", "Type", c => c.String());
            AlterColumn("dbo.Includes", "Content", c => c.String());
            AlterColumn("dbo.Includes", "Name", c => c.String());
            DropColumn("dbo.Includes", "Updated");
        }
    }
}
