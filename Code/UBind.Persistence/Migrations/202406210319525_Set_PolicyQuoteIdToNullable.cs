// <copyright file="202406210319525_Set_PolicyQuoteIdToNullable.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations;

using System.Data.Entity.Migrations;

public partial class Set_PolicyQuoteIdToNullable : DbMigration
{
    public override void Up()
    {
        this.DropIndex("dbo.PolicyReadModels", "IX_Policies_QuoteId");
        this.AlterColumn("dbo.PolicyReadModels", "QuoteId", c => c.Guid(nullable: true));
        this.AlterColumn("dbo.PolicyTransactions", "QuoteId", c => c.Guid(nullable: true));
        this.CreateIndex("dbo.PolicyReadModels", "QuoteId", name: "IX_Policies_QuoteId");
    }

    public override void Down()
    {
        this.DropIndex("dbo.PolicyReadModels", "IX_Policies_QuoteId");
        this.AlterColumn("dbo.PolicyTransactions", "QuoteId", c => c.Guid(nullable: false));
        this.AlterColumn("dbo.PolicyReadModels", "QuoteId", c => c.Guid(nullable: false));
        this.CreateIndex("dbo.PolicyReadModels", "QuoteId", name: "IX_Policies_QuoteId");
    }
}
