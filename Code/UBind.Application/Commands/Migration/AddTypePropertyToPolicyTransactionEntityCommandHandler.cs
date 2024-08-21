// <copyright file="AddTypePropertyToPolicyTransactionEntityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Adds the 'Type' property to PolicyTransaction Entity and add default values to it based on the value of
    /// 'Discriminator' property.
    /// </summary>
    public class AddTypePropertyToPolicyTransactionEntityCommandHandler : ICommandHandler<AddTypePropertyToPolicyTransactionEntityCommand, Unit>
    {
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ILogger<AddTypePropertyToPolicyTransactionEntityCommandHandler> logger;
        private readonly IUBindDbContext dbContext;

        public AddTypePropertyToPolicyTransactionEntityCommandHandler(
            IBackgroundJobClient backgroundJobClient,
            IUBindDbContext dbContext,
            ILogger<AddTypePropertyToPolicyTransactionEntityCommandHandler> logger)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(AddTypePropertyToPolicyTransactionEntityCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.backgroundJobClient.Enqueue(() => this.Process());
            return Task.FromResult(Unit.Value);
        }

        [JobDisplayName("Syncing values from Discriminator to Type column")]
        public void Process()
        {
            int batchSize = 1000;
            bool @continue = true;
            int totalRecordsModified = 0;
            while (@continue)
            {
                string sqlUpdate = $@"UPDATE dbo.PolicyTransactions
                              SET Type = CASE
                                  WHEN Discriminator = 'RenewalTransaction' THEN {(int)TransactionType.Renewal}
                                  WHEN Discriminator = 'AdjustmentTransaction' THEN {(int)TransactionType.Adjustment}
                                  WHEN Discriminator = 'CancellationTransaction' THEN {(int)TransactionType.Cancellation}
                              END 
                              WHERE Id IN (SELECT TOP {batchSize} Id FROM dbo.PolicyTransactions WHERE Discriminator != 'NewBusinessTransaction' AND Type = 0)";

                var modifiedRows = this.dbContext.Database.ExecuteSqlCommand(sqlUpdate);
                @continue = modifiedRows > 0 ? true : false;
                totalRecordsModified += modifiedRows;
                if (totalRecordsModified % 1000 == 0)
                {
                    this.logger.LogInformation($"{totalRecordsModified} Processed.");
                }

                Thread.Sleep(5000);
            }
        }
    }
}
