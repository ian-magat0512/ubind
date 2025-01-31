// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Refactoredreleases : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Releases", "Details_Id", "dbo.ReleaseDetails");
            DropIndex("dbo.Releases", new[] { "Details_Id" });
            CreateTable(
                "dbo.ReleaseEvents",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        ErrorMessagesJson = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Details_Id = c.Guid(),
                        Release_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReleaseDetails", t => t.Details_Id)
                .ForeignKey("dbo.Releases", t => t.Release_Id)
                .Index(t => t.Details_Id)
                .Index(t => t.Release_Id);
            
            DropColumn("dbo.Releases", "Details_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Releases", "Details_Id", c => c.Guid());
            DropForeignKey("dbo.ReleaseEvents", "Release_Id", "dbo.Releases");
            DropForeignKey("dbo.ReleaseEvents", "Details_Id", "dbo.ReleaseDetails");
            DropIndex("dbo.ReleaseEvents", new[] { "Release_Id" });
            DropIndex("dbo.ReleaseEvents", new[] { "Details_Id" });
            DropTable("dbo.ReleaseEvents");
            CreateIndex("dbo.Releases", "Details_Id");
            AddForeignKey("dbo.Releases", "Details_Id", "dbo.ReleaseDetails", "Id");
        }
    }
}
