﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class UpdateSystemEmailTemplatePortalIdTypeInDbForMultiNodeDeployment : DbMigration
    {
        public override void Up()
        {
            // Currently it's type is 'uniqueidentifer, null'  so temporarily set this back to nvarcharmax so that
            // undeployed nodes will not have errors
            this.Sql("ALTER TABLE dbo.SystemEmailTemplates alter column PortalId nvarchar(max) null");

            // Create a startup job to set it Back to a GUID type, but have the startup job run manually in
            // multi-node environments
            var createStartupJobRecord = StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJobV1(
                "SetSystemEmailTemplatePortalIdToBackToNullableGuid_20211105",
                false,
                true);
            Sql(createStartupJobRecord);
        }

        public override void Down()
        {
            // Remove startup job
            Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(
                "SetSystemEmailTemplatePortalIdToBackToNullableGuid_20211105"));
        }
    }
}
