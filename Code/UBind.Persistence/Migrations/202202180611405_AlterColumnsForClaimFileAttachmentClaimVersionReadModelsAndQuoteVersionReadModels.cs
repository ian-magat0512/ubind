﻿// <auto-generated />
namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlterColumnsForClaimFileAttachmentClaimVersionReadModelsAndQuoteVersionReadModels : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ClaimVersionReadModels");
            DropPrimaryKey("dbo.QuoteVersionReadModels");
            AlterColumn("dbo.ClaimFileAttachments", "CreationTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            AlterColumn("dbo.ClaimVersionReadModels", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.QuoteVersionReadModels", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.ClaimVersionReadModels", "Id");
            AddPrimaryKey("dbo.QuoteVersionReadModels", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.QuoteVersionReadModels");
            DropPrimaryKey("dbo.ClaimVersionReadModels");
            AlterColumn("dbo.QuoteVersionReadModels", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.ClaimVersionReadModels", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.ClaimFileAttachments", "CreationTimeInTicksSinceEpoch", c => c.Long());
            AddPrimaryKey("dbo.QuoteVersionReadModels", "Id");
            AddPrimaryKey("dbo.ClaimVersionReadModels", "Id");
        }
    }
}
