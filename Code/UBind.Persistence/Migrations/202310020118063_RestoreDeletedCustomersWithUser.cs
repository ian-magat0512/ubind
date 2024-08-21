// <copyright file="202310020118063_RestoreDeletedCustomersWithUser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class RestoreDeletedCustomersWithUser : DbMigration
    {
        private readonly string startupJobName = "RestoreDeletedCustomersWithUser_20231002";

        public override void Up()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(this.startupJobName, runManuallyIfInMultiNode: true));
        }

        public override void Down()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(this.startupJobName));
        }
    }
}
