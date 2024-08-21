// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PersistFileContentInSeparateTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileContents",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Content = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.QuoteDocumentReadModels", "SizeInBytes", c => c.Long(nullable: false));
            AddColumn("dbo.QuoteDocumentReadModels", "FileContentId", c => c.Guid(nullable: false));
            DropColumn("dbo.QuoteDocumentReadModels", "Content");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuoteDocumentReadModels", "Content", c => c.Binary());
            DropColumn("dbo.QuoteDocumentReadModels", "FileContentId");
            DropColumn("dbo.QuoteDocumentReadModels", "SizeInBytes");
            DropTable("dbo.FileContents");
        }
    }
}
