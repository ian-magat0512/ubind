﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class UpdateUserAggregateCustomerId : DbMigration
    {
        private string startupJobAlias = "UpdateUserAggregateCustomerId_20220831";
        public override void Up()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJobV1(this.startupJobAlias, runManuallyIfInMultiNode: true));
        }

        public override void Down()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(this.startupJobAlias));
        }
    }
}
