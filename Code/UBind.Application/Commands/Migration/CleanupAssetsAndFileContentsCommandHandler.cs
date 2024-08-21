// <copyright file="CleanupAssetsAndFileContentsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class CleanupAssetsAndFileContentsCommandHandler
        : ICommandHandler<CleanupAssetsAndFileContentsCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ILogger<CleanupAssetsAndFileContentsCommandHandler> logger;

        public CleanupAssetsAndFileContentsCommandHandler(
            IUBindDbContext dbContext, IBackgroundJobClient backgroundJobClient, ILogger<CleanupAssetsAndFileContentsCommandHandler> logger)
        {
            this.dbContext = dbContext;
            this.backgroundJobClient = backgroundJobClient;
            this.logger = logger;
        }

        public Task<Unit> Handle(CleanupAssetsAndFileContentsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.backgroundJobClient.Enqueue(() => this.CleanupAssetsAndFileContents());
            return Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// Cleanup file contents.
        /// </summary>
        [JobDisplayName("Startup Job: Cleanup asset and file contents")]
        public void CleanupAssetsAndFileContents()
        {
            this.dbContext.Database.ExecuteSqlCommand("ALTER TABLE dbo.Assets DROP COLUMN IF EXISTS[Content];");

            this.logger.LogInformation($"Cleaning up FileContents...");
            var sqlFileContentsWithoutTenant = @"SELECT Id FROM FileContents f
WHERE NOT EXISTS (SELECT 1 FROM Tenants t WHERE t.Id=f.TenantId)
AND NOT EXISTS(SELECT 1 FROM DocumentFiles WHERE FileContentId=f.Id)
AND NOT EXISTS(SELECT 1 FROM QuoteDocumentReadModels WHERE FileContentId=f.Id)
AND NOT EXISTS(SELECT 1 FROM ClaimFileAttachments WHERE FileContentId=f.Id)
AND NOT EXISTS(SELECT 1 FROM Assets WHERE FileContentId=f.Id)
AND NOT EXISTS(SELECT 1 FROM QuoteFileAttachments WHERE FileContentId=f.Id)";

            var fileContentsWithoutTenant = this.dbContext.Database.SqlQuery<Guid>(sqlFileContentsWithoutTenant).ToList();
            var rowCount = fileContentsWithoutTenant.Count;

            // We're doing this one by one because this was very slow during testing and filling up the DB log
            var ctr = 0;
            foreach (var f in fileContentsWithoutTenant)
            {
                this.logger.LogInformation($"Deleting {f}, {++ctr}/{rowCount}");
                var sqlDelete = $"DELETE FROM FileContents WHERE Id='{f}'";
                this.dbContext.Database.ExecuteSqlCommand(sqlDelete);
                if (ctr % 500 == 0)
                {
                    this.ShrinkLog();
                    this.MemoryCleanup();
                    Thread.Sleep(200);
                }
            }

            this.logger.LogInformation($"Cleaned up FileContents.");
        }

        private void ShrinkLog()
        {
            this.logger.LogInformation("Shrinking DB log...");
            this.dbContext.Database.ExecuteSqlCommand(
                TransactionalBehavior.DoNotEnsureTransaction, "DBCC SHRINKFILE (2, TRUNCATEONLY)");
        }

        private void MemoryCleanup()
        {
            var memBefore = BytesToGb(GC.GetTotalMemory(false));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
            GC.WaitForPendingFinalizers();
            var memAfter = BytesToGb(GC.GetTotalMemory(true));

            this.logger.LogInformation(
                $"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");

            double BytesToGb(long bytes)
            {
                return bytes * 1E-9;
            }
        }
    }
}
