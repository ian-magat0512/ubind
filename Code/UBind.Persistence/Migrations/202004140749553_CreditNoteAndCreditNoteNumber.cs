// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreditNoteAndCreditNoteNumber : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CreditNoteNumbers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TenantId = c.String(),
                        ProductId = c.String(),
                        Environment = c.Int(nullable: false),
                        Number = c.String(),
                        IsAssigned = c.Boolean(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Quotes", "IsCreditNoteIssued", c => c.Boolean(nullable: false));
            AddColumn("dbo.Quotes", "CreditNoteNumber", c => c.String());
            AddColumn("dbo.Quotes", "CreditNoteTimeAsTicksSinceEpoch", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quotes", "CreditNoteTimeAsTicksSinceEpoch");
            DropColumn("dbo.Quotes", "CreditNoteNumber");
            DropColumn("dbo.Quotes", "IsCreditNoteIssued");
            DropTable("dbo.CreditNoteNumbers");
        }
    }
}
