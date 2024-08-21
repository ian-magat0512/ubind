// <copyright file="UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Persistence;

    /// <summary>
    /// Command handler when updating records and creating indexes for date and timestamp renames,
    /// This is necessary to run before before RenameDateFieldsCommandHandler because this is the offset coming from the migration.
    /// </summary>
    public class UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommandHandler
        : ICommandHandler<UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommand, Unit>
    {
        private readonly ILogger<RenameDateFieldsCleanupCommandHandler> logger;
        private readonly IUBindDbContext dbContext;

        public UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommandHandler(
            IUBindDbContext dbContext,
            ILogger<RenameDateFieldsCleanupCommandHandler> logger)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public Task<Unit> Handle(UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure nullable timestamp columns with a 0 value are set to null
            RenameDateFieldsData.GetTimestampColumnRenames().ForEach(cr =>
            {
                var tableName = SqlHelper.WithSchema(cr.TableName);
                if (cr.NewNullable)
                {
                    this.logger.LogInformation($"Setting {tableName}.{cr.NewColumnName} values to NULL when they are 0.");
                    this.dbContext.ExecuteSqlScript(
                        $"UPDATE {tableName} SET {cr.NewColumnName} = NULL WHERE {cr.NewColumnName} = 0", 0);
                }
            });

            // Copy the date time to the ticks column
            this.logger.LogInformation("Updating claimsReadModels ticks from datetime.");
            var updateIncidentTicks = "UPDATE dbo.ClaimReadModels SET IncidentTicksSinceEpoch = dbo.ToTicks(IncidentDateAsDateTime);";
            this.dbContext.ExecuteSqlScript(updateIncidentTicks, 0);

            // For Cancellation transactions, copy the cancellation timestamp to the effective timestamp
            this.logger.LogInformation("Updating policyTransations effectiveTicks from cancellationTicks.");
            var updateEffectiveTime = "UPDATE dbo.PolicyTransactions SET EffectiveTicksSinceEpoch = CancellationTimeAsTicksSinceEpoch "
                + "WHERE Discriminator = 'CancellationTransaction' "
                + "AND CancellationTimeAsTicksSinceEpoch > 0";
            this.dbContext.ExecuteSqlScript(updateEffectiveTime, 0);

            // Copy the Ticks to the datetime column
            this.logger.LogInformation("Updating policyTransations effectiveDateTime from effectiveTicks.");
            var updateEffectiveDateTime = "UPDATE dbo.PolicyTransactions SET EffectiveDateTime = dbo.ToDateTime2(EffectiveTicksSinceEpoch);";
            this.dbContext.ExecuteSqlScript(updateEffectiveDateTime, 0);

            return Task.FromResult(Unit.Value);
        }
    }
}
