﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrganisationFeature : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrganisationReadModels",
                c => new
                    {
                        OrganisationId = c.Guid(nullable: false),
                        TenantId = c.String(),
                        Alias = c.String(),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.OrganisationId);
            
            AddColumn("dbo.TenantDetails", "DefaultOrganisationId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TenantDetails", "DefaultOrganisationId");
            DropTable("dbo.OrganisationReadModels");
        }
    }
}
