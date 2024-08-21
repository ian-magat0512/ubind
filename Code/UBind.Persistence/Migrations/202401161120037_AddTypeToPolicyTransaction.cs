// <copyright file="202401161120037_AddTypeToPolicyTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;
    using UBind.Persistence.Migrations.Extensions;

    public partial class AddTypeToPolicyTransaction : DbMigration
    {
        private const string AddTypePropertyToPolicyTransactionEntityStartupJobName =
            "AddTypePropertyToPolicyTransactionEntity_20240110";

        public override void Up()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                AddTypePropertyToPolicyTransactionEntityStartupJobName,
                blocking: false,
                runManuallyIfInMultiNode: true));
            this.AddColumnIfNotExists("dbo.PolicyTransactions", "Type", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            this.Sql(StartupJobRunnerQueryHelper
            .GenerateDeleteQueryForStartupJob(AddTypePropertyToPolicyTransactionEntityStartupJobName));
            this.DropColumn("dbo.PolicyTransactions", "Type");
        }
    }
}
