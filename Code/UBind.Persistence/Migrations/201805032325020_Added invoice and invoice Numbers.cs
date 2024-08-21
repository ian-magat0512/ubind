// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvoiceandInvoiceNumbers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvoiceNumbers",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    ProductId = c.String(),
                    Environment = c.Int(nullable: false),
                    Number = c.String(),
                    IsAssigned = c.Boolean(nullable: false),
                    CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Invoices",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    InvoiceNumber = c.String(),
                    CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    SubmittedFormUpdateId = c.Guid(nullable: false),
                    SubmittedCalculationResultId = c.Guid(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Applications", "Invoice_Id", c => c.Guid());
            CreateIndex("dbo.Applications", "Invoice_Id");
            AddForeignKey("dbo.Applications", "Invoice_Id", "dbo.Invoices", "Id");

            AlterColumn("dbo.InvoiceNumbers", "ProductId", c => c.String(maxLength: 255, unicode: false));
            AlterColumn("dbo.InvoiceNumbers", "Number", c => c.String(maxLength: 100, unicode: false));
            CreateIndex("dbo.InvoiceNumbers", new[] { "ProductId", "Environment", "Number" }, unique: true, name: "AK_InvoiceNumberProductEnvironmentAndNumberIndex");
        }
        
        public override void Down()
        {
            DropTable("dbo.Invoices");
            DropTable("dbo.InvoiceNumbers");
            DropForeignKey("dbo.Applications", "Invoice_Id", "dbo.Invoices");
            DropIndex("dbo.Applications", new[] { "Invoice_Id" });
            DropColumn("dbo.Applications", "Invoice_Id");
            DropIndex("dbo.InvoiceNumbers", "AK_InvoiceNumberProductEnvironmentAndNumberIndex");
            AlterColumn("dbo.InvoiceNumbers", "Number", c => c.String());
            AlterColumn("dbo.InvoiceNumbers", "ProductId", c => c.String());
        }
    }
}
