// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedfileattachment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileAttachments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Type = c.String(),
                        Content = c.String(),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Application_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Applications", t => t.Application_Id)
                .Index(t => t.Application_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FileAttachments", "Application_Id", "dbo.Applications");
            DropIndex("dbo.FileAttachments", new[] { "Application_Id" });
            DropTable("dbo.FileAttachments");
        }
    }
}
