﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class ChangeCustomerReadModelOwnerUserIdToNullable : DbMigration
    {
        private readonly string alias = "UpdateCustomerOwnerUserIdFromDefaultGuidToNull_20221205";
        public override void Up()
        {
            // Make the OwnerUserId column nullable. Note that a separate StartupJob migration
            // will update default guid values to null in batches.
            AlterColumn("dbo.CustomerReadModels", "OwnerUserId", c => c.Guid(nullable: true));

            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(this.alias, runManuallyIfInMultiNode: true));
        }

        public override void Down()
        {
            AlterColumn("dbo.CustomerReadModels", "OwnerUserId", c => c.Guid(nullable: false));
        }
    }
}
