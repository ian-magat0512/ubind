// <copyright file="202303240638593_CleanupDocumentFileContents.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class CleanupDocumentFileContents : DbMigration
    {
        private const string CleanupDocumentFileContentsAlias = "CleanupDocumentFileContents";

        public override void Up()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                CleanupDocumentFileContentsAlias, runManuallyIfInMultiNode: true));
        }

        public override void Down()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(CleanupDocumentFileContentsAlias));
        }
    }
}
