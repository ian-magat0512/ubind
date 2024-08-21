// <copyright file="202408130241567_UpdateQuoteDocumentsCleanUp.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class UpdateQuoteDocumentsCleanUp : DbMigration
    {
        private const string StartupJobAliasForCustomEventAlias = "UpdateQuoteDocumentsQuoteOrPolicyTransactionId_20240813";

        public override void Up()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(StartupJobAliasForCustomEventAlias));
        }

        public override void Down()
        {
        }
    }
}
