﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Migrations.Extensions;

    public partial class AddTenantNewIdOnClaimAttachmentsAndPortals : DbMigration
    {
        public override void Up()
        {
            this.AddColumnIfNotExists("dbo.ClaimFileAttachments", "TenantNewId", c => c.Guid(nullable: false));
            this.AddColumnIfNotExists("dbo.Portals", "TenantNewId", c => c.Guid(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Portals", "TenantNewId");
            DropColumn("dbo.ClaimFileAttachments", "TenantNewId");
        }
    }
}
