// <copyright file="AddUserDeletedEventsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class AddUserDeletedEventsCommandHandler
    : ICommandHandler<AddUserDeletedEventsCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly ILogger<CleanupAssetsAndFileContentsCommandHandler> logger;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IUserAggregateRepository userAggregateRepository;
    private readonly string tmpTable = "TmpDeletedPersonAggregates";

    public AddUserDeletedEventsCommandHandler(
        IUBindDbContext dbContext,
        ILogger<CleanupAssetsAndFileContentsCommandHandler> logger,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IUserAggregateRepository userAggregateRepository)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.userAggregateRepository = userAggregateRepository;
    }

    [JobDisplayName("Add User Deleted Events")]
    public async Task<Unit> Handle(AddUserDeletedEventsCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => this.HandleCancellationRequest());
        await this.AddUserDeletedEvents(cancellationToken);
        return Unit.Value;
    }

    private async Task AddUserDeletedEvents(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Started {DateTime.Now}");
        this.PopulateTempTable();
        var sqlRowsToProcess =
            $"SELECT TenantId, UserId, LastModifiedTicksSinceEpoch FROM {this.tmpTable} WITH (NOLOCK) WHERE IsProcessed<>1";
        var eventsToProcess = this.dbContext.Database.SqlQuery<DeletedPersonReadModels>(sqlRowsToProcess).ToList();
        int ctr = 0;
        foreach (var e in eventsToProcess)
        {
            var userAggregate = this.userAggregateRepository.GetById(e.TenantId, e.UserId);
            userAggregate.SoftDelete(this.httpContextPropertiesResolver.PerformingUserId, Instant.FromUnixTimeTicks(e.LastModifiedTicksSinceEpoch));
            await this.userAggregateRepository.Save(userAggregate);

            var setProcessedFlag = $@"UPDATE {this.tmpTable} SET IsProcessed=1 WHERE TenantId='{e.TenantId}' AND UserId='{e.UserId}';";
            this.dbContext.Database.ExecuteSqlCommand(setProcessedFlag);
            this.logger.LogInformation(
                $"{++ctr}/{eventsToProcess.Count} added user deleted event for " +
                $"TenantId: {e.TenantId}, User ID: {e.UserId}");

            // Delay for each record so we don't overwhelm the database
            await Task.Delay(500, cancellationToken);
        }

        this.dbContext.Database.ExecuteSqlCommand($"DROP TABLE IF EXISTS {this.tmpTable};");
        this.logger.LogInformation($"Finished {DateTime.Now}");
    }

    private void PopulateTempTable()
    {
        if (this.TableExists(this.tmpTable))
        {
            return;
        }

        var dbTimeout = this.dbContext.Database.CommandTimeout;
        try
        {
            this.dbContext.Database.CommandTimeout = 0;
            this.logger.LogInformation($"Populating temp table '{this.tmpTable}' to process...");

            // Get deleted person readmodels with NON NULL User ID
            var sqlToProcess = $@"SELECT TenantId, UserId, LastModifiedTicksSinceEpoch, 0 IsProcessed
INTO {this.tmpTable}
FROM PersonReadModels WITH (NOLOCK)
WHERE IsDeleted = 1 AND UserId IS NOT NULL";
            this.dbContext.Database.ExecuteSqlCommand(sqlToProcess);
            this.logger.LogInformation($"'{this.tmpTable}' created.");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            this.dbContext.Database.CommandTimeout = dbTimeout;
        }
    }

    private void HandleCancellationRequest()
    {
        this.logger.LogInformation("AddUserDeletedEvents job was canceled.");
        throw new OperationCanceledException();
    }

    private bool TableExists(string tableName)
    {
        bool exists = this.dbContext.Database
            .SqlQuery<int?>($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'")
            .SingleOrDefault() != null;
        return exists;
    }

    private class DeletedPersonReadModels
    {
        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public long LastModifiedTicksSinceEpoch { get; set; }
    }
}
