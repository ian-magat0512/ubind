﻿// <copyright file="202301302334204_CleanupAssetFilesAndPopulateDocumentFiles.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;
    using UBind.Persistence.Migrations.Extensions;

    public partial class CleanupAssetFilesAndPopulateDocumentFiles : DbMigration
    {
        private const string CleanupStartupJobAlias = "InsertQuoteAndClaimFileContentsByTenant";
        private readonly string populateFileContentsFromDocumentFiles = "PopulateFileContentsFromDocumentFiles";
        private readonly string cleanupAssetsAndFileContents = "CleanupAssetsAndFileContents";

        public override void Up()
        {
            this.Up_CleanupAssetsAndFileContents();
            this.Up_PopulateFileContentsFromDocumentFiles();
        }

        public override void Down()
        {
            this.Down_PopulateFileContentsFromDocumentFiles();
            this.Down_CleanupAssetsAndFileContents();
        }

        private void Up_CleanupAssetsAndFileContents()
        {
            // Drop old hashcode index
            this.DropIndex("dbo.FileContents", "IX_FileContents_HashCode");

            // Create index on TenantId & HashCode column
            this.CreateIndexDropFirstIfExists(
                "dbo.FileContents", new[] { "TenantId", "HashCode" }, false, "IX_FileContents_TenantIdHashCode");

            // Startup job to insert qutoe and claim file contents by tenant.
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(CleanupStartupJobAlias));

            // Startup job to drop Assets.Content and cleanup and file contents
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                this.cleanupAssetsAndFileContents, runManuallyIfInMultiNode: true));
        }

        private void Down_CleanupAssetsAndFileContents()
        {
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(CleanupStartupJobAlias));
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(this.cleanupAssetsAndFileContents));
        }

        private void Up_PopulateFileContentsFromDocumentFiles()
        {
            this.AddColumnIfNotExists("dbo.DocumentFiles", "FileContentId", c => c.Guid(nullable: false));
            this.CreateIndex("dbo.DocumentFiles", "FileContentId");
            this.AddForeignKey("dbo.DocumentFiles", "FileContentId", "dbo.FileContents", "Id", cascadeDelete: true);

            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(this.populateFileContentsFromDocumentFiles));
        }

        private void Down_PopulateFileContentsFromDocumentFiles()
        {
            this.DropForeignKey("dbo.DocumentFiles", "FileContentId", "dbo.FileContents");
            this.DropIndex("dbo.DocumentFiles", new[] { "FileContentId" });
            this.DropColumn("dbo.DocumentFiles", "FileContentId");

            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(this.populateFileContentsFromDocumentFiles));
        }
    }
}
