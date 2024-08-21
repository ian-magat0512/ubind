// <copyright file="CustomerUserOwnerIdMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Linq;
    using System.Threading;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class CustomerUserOwnerIdMigration : ICustomerUserOwnerIdMigration
    {
        private const int RecordsPerBatch = 5000;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<CustomerUserOwnerIdMigration> logger;

        public CustomerUserOwnerIdMigration(
            IUBindDbContext dbContext,
            ILogger<CustomerUserOwnerIdMigration> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void ProcessUpdatingCustomerUserOwnerId()
        {
            this.logger.LogInformation($"Migration started for customer user owner id update.");
            this.ProcessBatchUpdate(1);
        }

        [JobDisplayName("Startup Job: ProcessUpdatingCustomerUserOwnerId Process Batch {0}")]
        private void ProcessBatchUpdate(int batch)
        {
            var countQuery = $"SELECT COUNT(*) FROM CustomerReadModels WITH (NOLOCK) WHERE OwnerUserId = '{Guid.Empty}'";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            int affectedRows;
            do
            {
                this.logger.LogInformation($"REMAINING ROWS TO PROCESS: {totalRecordsToProcess}, Batch: {batch}, Size: {RecordsPerBatch}");
                var updateSql = $"UPDATE TOP({RecordsPerBatch}) dbo.CustomerReadModels "
                    + "SET OwnerUserId=NULL "
                    + $"WHERE OwnerUserId='{Guid.Empty}'";
                affectedRows = this.dbContext.ExecuteSqlScript(updateSql);

                batch++;
                Thread.Sleep(200);
            }
            while (affectedRows == RecordsPerBatch);
        }
    }
}
