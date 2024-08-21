﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class CreateLoginAttemptEmailAddressAndOrganisationIdIndex : DbMigration
    {
        public override void Up()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"CreateLoginAttemptEmailAddressAndOrganisationIdIndex.sql");
            this.Sql(File.ReadAllText(sqlFile));
        }

        public override void Down()
        {
            this.Sql("DROP index if exists [dbo].LoginAttemptResults.IX_LoginAttemptResults_TenantId_EmailAddress_OrganisationId");
        }
    }
}
