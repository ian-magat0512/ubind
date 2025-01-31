﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RefactorEmailRelationships : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.QuoteEmailModels", newName: "Emails");
            CreateTable(
                "dbo.Relationships",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    FromEntityType = c.Int(nullable: false),
                    FromEntityId = c.Guid(nullable: false),
                    Type = c.Int(nullable: false),
                    ToEntityType = c.Int(nullable: false),
                    ToEntityId = c.Guid(nullable: false),
                    CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Tags",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    EntityId = c.Guid(nullable: false),
                    EntityType = c.Int(nullable: false),
                    TagType = c.Int(nullable: false),
                    Value = c.String(),
                    CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Emails", "TenantId", c => c.String());
            AddColumn("dbo.Emails", "ProductId", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Emails", "ProductId");
            DropColumn("dbo.Emails", "TenantId");
            DropTable("dbo.Tags");
            DropTable("dbo.Relationships");
            RenameTable(name: "dbo.Emails", newName: "QuoteEmailModels");
        }
    }
}
