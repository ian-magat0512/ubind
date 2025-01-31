﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PolicyCancellation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Policies", "CancellationDate", c => c.String());
            AddColumn("dbo.PolicyReadModels", "CancellationDateAsDateTime", c => c.DateTime());
            AddColumn("dbo.PolicyTransactions", "PolicyData_CancellationTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            AddColumn("dbo.PolicyTransactions", "PolicyData_CancellationDateAsDateTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.PolicyTransactions", "CancellationTimeAsTicksSinceEpoch", c => c.Long());
            AddColumn("dbo.PolicyTransactions", "CancellationDateAsDateTime", c => c.DateTime());
            AlterColumn("dbo.PolicyTransactions", "QuoteId", c => c.Guid(nullable: false));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_InceptionTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_ExpiryTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_InceptionDateAsDateTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_ExpiryDateAsDateTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PolicyTransactions", "PolicyData_ExpiryDateAsDateTime", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_InceptionDateAsDateTime", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.PolicyTransactions", "PolicyData_ExpiryTimeInTicksSinceEpoch", c => c.Long());
            AlterColumn("dbo.PolicyTransactions", "PolicyData_InceptionTimeInTicksSinceEpoch", c => c.Long());
            AlterColumn("dbo.PolicyTransactions", "QuoteId", c => c.Guid());
            DropColumn("dbo.PolicyTransactions", "CancellationDateAsDateTime");
            DropColumn("dbo.PolicyTransactions", "CancellationTimeAsTicksSinceEpoch");
            DropColumn("dbo.PolicyTransactions", "PolicyData_CancellationDateAsDateTime");
            DropColumn("dbo.PolicyTransactions", "PolicyData_CancellationTimeInTicksSinceEpoch");
            DropColumn("dbo.PolicyReadModels", "CancellationDateAsDateTime");
            DropColumn("dbo.Policies", "CancellationDate");
        }
    }
}
