// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUseCaseForUniqueNumberSequences : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.QuoteNumberSequences");
            AddColumn("dbo.QuoteNumberSequences", "UseCase", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.QuoteNumberSequences", new[] { "TenantId", "ProductId", "Environment", "Method", "Number", "UseCase" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.QuoteNumberSequences");
            DropColumn("dbo.QuoteNumberSequences", "UseCase");
            AddPrimaryKey("dbo.QuoteNumberSequences", new[] { "TenantId", "ProductId", "Environment", "Method", "Number" });
        }
    }
}
