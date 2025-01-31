﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBasicAccounting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvoiceObsoletes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InvoiceNumber = c.String(),
                        SubmittedFormUpdateId = c.Guid(nullable: false),
                        SubmittedCalculationResultId = c.Guid(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentAllocationReadModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CommercialDocument_Id = c.Guid(),
                        FinancialTransaction_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Invoices", t => t.CommercialDocument_Id)
                .ForeignKey("dbo.PaymentReadModels", t => t.FinancialTransaction_Id)
                .Index(t => t.CommercialDocument_Id)
                .Index(t => t.FinancialTransaction_Id);
            
            CreateTable(
                "dbo.PaymentReadModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(),
                        PayeeId = c.Guid(),
                        PayeeType = c.Int(),
                        PayerId = c.Guid(nullable: false),
                        PayerType = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        LastUpdatedTicksSinceEpoch = c.Long(nullable: false),
                        CreationTimeAsTicksSinceEpoch = c.Long(nullable: false),
                        TransactionTimeAsTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RefundAllocationReadModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CommercialDocument_Id = c.Guid(),
                        FinancialTransaction_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CreditNotes", t => t.CommercialDocument_Id)
                .ForeignKey("dbo.RefundReadModels", t => t.FinancialTransaction_Id)
                .Index(t => t.CommercialDocument_Id)
                .Index(t => t.FinancialTransaction_Id);
            
            CreateTable(
                "dbo.CreditNotes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BreakdownId = c.Guid(),
                        DueDateTimeInTicksSinceEpoch = c.Long(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RefundReadModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(),
                        PayeeId = c.Guid(),
                        PayeeType = c.Int(),
                        PayerId = c.Guid(nullable: false),
                        PayerType = c.Int(nullable: false),
                        ReferenceNumber = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                        LastUpdatedTicksSinceEpoch = c.Long(nullable: false),
                        CreationTimeAsTicksSinceEpoch = c.Long(nullable: false),
                        TransactionTimeAsTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Invoices", "BreakdownId", c => c.Guid());
            AddColumn("dbo.Invoices", "DueDateTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            DropColumn("dbo.Invoices", "InvoiceNumber");
            DropColumn("dbo.Invoices", "SubmittedFormUpdateId");
            DropColumn("dbo.Invoices", "SubmittedCalculationResultId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Invoices", "SubmittedCalculationResultId", c => c.Guid(nullable: false));
            AddColumn("dbo.Invoices", "SubmittedFormUpdateId", c => c.Guid(nullable: false));
            AddColumn("dbo.Invoices", "InvoiceNumber", c => c.String());
            DropForeignKey("dbo.RefundAllocationReadModels", "FinancialTransaction_Id", "dbo.RefundReadModels");
            DropForeignKey("dbo.RefundAllocationReadModels", "CommercialDocument_Id", "dbo.CreditNotes");
            DropForeignKey("dbo.PaymentAllocationReadModels", "FinancialTransaction_Id", "dbo.PaymentReadModels");
            DropForeignKey("dbo.PaymentAllocationReadModels", "CommercialDocument_Id", "dbo.Invoices");
            DropIndex("dbo.RefundAllocationReadModels", new[] { "FinancialTransaction_Id" });
            DropIndex("dbo.RefundAllocationReadModels", new[] { "CommercialDocument_Id" });
            DropIndex("dbo.PaymentAllocationReadModels", new[] { "FinancialTransaction_Id" });
            DropIndex("dbo.PaymentAllocationReadModels", new[] { "CommercialDocument_Id" });
            DropColumn("dbo.Invoices", "DueDateTimeInTicksSinceEpoch");
            DropColumn("dbo.Invoices", "BreakdownId");
            DropTable("dbo.RefundReadModels");
            DropTable("dbo.CreditNotes");
            DropTable("dbo.RefundAllocationReadModels");
            DropTable("dbo.PaymentReadModels");
            DropTable("dbo.PaymentAllocationReadModels");
            DropTable("dbo.InvoiceObsoletes");
        }
    }
}
