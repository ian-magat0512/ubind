﻿// <auto-generated />
#pragma warning disable 159
namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;
    using UBind.Persistence.Migrations.Extensions;

    public partial class RemoveSystemEventEmittedFlag : DbMigration
    {
        public override void Up()
        {
            // We'll do this in a Startup Job so it can happen after deployment to all nodes
            /* DropColumn("dbo.SystemEvents", "IsEmitted"); */
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                "RemoveSystemEventEmittedFlag_20230225", false, true));

        }

        public override void Down()
        {
            this.AddColumnIfNotExists("dbo.SystemEvents", "IsEmitted", c => c.Boolean(nullable: false));
            this.Sql(
                StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob("RemoveSystemEventEmittedFlag_20230225"));
        }
    }
}
