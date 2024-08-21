// <copyright file="SetClaimReadModelDatesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using System.Data.Entity.Migrations;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.Repositories;

/// <summary>
/// This is the handler for the migration command called on startup that
/// sets the claim lodged, settled, and declined dates.
/// </summary>
public class SetClaimReadModelDatesCommandHandler
    : ICommandHandler<SetClaimReadModelDatesCommand, Unit>
{
    private readonly IClaimReadModelRepository claimReadModelRepository;
    private readonly ILogger<SetClaimReadModelDatesCommandHandler> logger;
    private readonly IUBindDbContext dbContext;
    private readonly ITenantRepository tenantRepository;

    public SetClaimReadModelDatesCommandHandler(
        IUBindDbContext dbContext,
        ITenantRepository tenantRepository,
        IClaimReadModelRepository claimReadModelRepository,
        ILogger<SetClaimReadModelDatesCommandHandler> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.tenantRepository = tenantRepository;
        this.claimReadModelRepository = claimReadModelRepository;
    }

    public async Task<Unit> Handle(SetClaimReadModelDatesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Set lodged, settled or declined dates for Claims of tenant {tenant.Id}");
                await this.UpdateClaimReadModelByTenant(tenant.Id, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogInformation("Cancellation requested, exiting.");
            throw;
        }

        this.logger.LogInformation("Completed.");
        return Unit.Value;
    }

    private async Task UpdateClaimReadModelByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var pageNumber = 1;
        var pageSize = (int)PageSize.Default;
        var claimsProcessed = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var claims = this.GetClaimsToUpdate(tenantId, pageNumber, pageSize);
            await this.UpdateClaimReadModels(tenantId, claims, cancellationToken);
            claimsProcessed += claims.Count;
            this.logger.LogInformation($"Processed claims: {claims.Count}");
            if (claims.Count < pageSize)
            {
                break;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private List<ClaimReadModel> GetClaimsToUpdate(Guid tenantId, int pageNumber, int pageSize)
    {
        var claimStatuses = new List<string>
        {
            ClaimState.Review,
            ClaimState.Assessment,
            ClaimState.Approved,
            ClaimState.Declined,
            ClaimState.Complete,
            ClaimState.Withdrawn,
        };
        return this.claimReadModelRepository.ListClaimsWithLodgeSettledDeclinedDatesNotSet(tenantId, new EntityListFilters
        {
            IncludeTestData = false,
            Page = pageNumber,
            PageSize = pageSize,
            Statuses = claimStatuses,
            SortBy = nameof(ClaimReadModel.CreatedTicksSinceEpoch),
            SortOrder = Domain.Enums.SortDirection.Ascending,
        }).ToList();
    }

    private async Task UpdateClaimReadModels(Guid tenantId, List<ClaimReadModel> claims, CancellationToken cancellationToken)
    {
        var recordsSavingInterval = (int)PageSize.Normal;
        var numberOfRecordsToSave = 0;
        foreach (var c in claims)
        {
            int retryTimes = 0;
            async Task UpdateClaimReadModel(ClaimReadModel claim, CancellationToken cancellationToken)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }

                    bool isPendingUpdate = false;
                    var summaries = this.GetClaimStateChangedEventSummaries(claim.Id);
                    if (summaries.Count == 0)
                    {
                        return;
                    }

                    var lodgedDateTime = this.GetLodgedDate(summaries);
                    if (lodgedDateTime.HasValue)
                    {
                        claim.LodgedTimestamp = lodgedDateTime.Value;
                        isPendingUpdate = true;
                    }

                    var settledDateTime = this.GetSettledDate(claim, summaries);
                    if (settledDateTime.HasValue)
                    {
                        claim.SettledTimestamp = settledDateTime.Value;
                        isPendingUpdate = true;
                    }

                    var declinedDateTime = this.GetDeclinedDate(claim, summaries);
                    if (declinedDateTime.HasValue)
                    {
                        claim.DeclinedTimestamp = declinedDateTime.Value;
                        isPendingUpdate = true;
                    }

                    if (isPendingUpdate)
                    {
                        this.dbContext.ClaimReadModels.AddOrUpdate(claim);
                        numberOfRecordsToSave++;
                    }

                    if (numberOfRecordsToSave % recordsSavingInterval == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await this.dbContext.SaveChangesAsync();
                        this.logger.LogInformation($"Save changes, count: {recordsSavingInterval}");
                    }

                    await Task.Delay(2000, cancellationToken);
                }
                catch (Exception e)
                {
                    this.logger.LogError($"ERROR: Claim {claim.Id} for tenant {claim.TenantId}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                    throw;
                }

                await Task.Delay(50, cancellationToken);
            }

            await RetryPolicyHelper.ExecuteAsync<Exception>((ct) => UpdateClaimReadModel(c, ct), maxJitter: 1500, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (numberOfRecordsToSave % recordsSavingInterval != 0)
        {
            await this.SaveChangesForUnsavedRecords(numberOfRecordsToSave % recordsSavingInterval, cancellationToken);
        }
    }

    private async Task SaveChangesForUnsavedRecords(int noOfRecords, CancellationToken cancellationToken)
    {
        async Task Save(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.dbContext.SaveChangesAsync();
            this.logger.LogInformation($"Save changes, count: {noOfRecords}");
        }

        await RetryPolicyHelper.ExecuteAsync<Exception>((cancellationToken) => Save(cancellationToken), maxJitter: 1500, cancellationToken: cancellationToken);
    }

    private List<ClaimStateChangeEventSummary> GetClaimStateChangedEventSummaries(Guid aggregateId)
    {
        var summaries = new List<ClaimStateChangeEventSummary>();
        var list = this.dbContext.EventRecordsWithGuidIds
            .Where(record => record.AggregateId.Equals(aggregateId) && record.AggregateType == AggregateType.Claim)
            .OrderBy(record => record.Sequence);
        foreach (var eventRecord in list)
        {
            var summary = this.GetEventSummary(eventRecord);
            if (summary != null)
            {
                summaries.Add(summary);
            }
        }

        return summaries;
    }

    private ClaimStateChangeEventSummary GetEventSummary(EventRecordWithGuidId eventRecord)
    {
        var @event = eventRecord.GetEvent<IEvent<ClaimAggregate, Guid>, ClaimAggregate>();
        var stateChangedEvent = @event as ClaimAggregate.ClaimStateChangedEvent;
        if (stateChangedEvent == null)
        {
            return null;
        }

        return new ClaimStateChangeEventSummary
        {
            SequenceNumber = eventRecord.Sequence,
            Timestamp = eventRecord.Timestamp,
            ResultingClaimState = stateChangedEvent.ResultingState,
            PreviousClaimState = stateChangedEvent.OriginalState,
        };
    }

    private Instant? GetLodgedDate(List<ClaimStateChangeEventSummary> summaries)
    {
        var eventsResultingToLodgedClaim = summaries
            .Where(p => (p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Review)
                || p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Assessment)
                || p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Approved)
                || p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Complete)))
            .OrderBy(p => p.SequenceNumber);

        var firstLodgedEvent = eventsResultingToLodgedClaim.FirstOrDefault();
        if (firstLodgedEvent == null)
        {
            return null;
        }

        return firstLodgedEvent.Timestamp;
    }

    private Instant? GetSettledDate(ClaimReadModel claim, List<ClaimStateChangeEventSummary> summaries)
    {
        if (!claim.Status.EqualsIgnoreCase(ClaimState.Complete))
        {
            return null;
        }

        var latestEventSummary = summaries.Where(p => p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Complete))
            .OrderBy(p => p.SequenceNumber)
            .LastOrDefault();
        if (latestEventSummary == null)
        {
            return null;
        }

        return latestEventSummary.Timestamp;
    }

    private Instant? GetDeclinedDate(ClaimReadModel claim, List<ClaimStateChangeEventSummary> summaries)
    {
        if (!claim.Status.EqualsIgnoreCase(ClaimState.Declined))
        {
            return null;
        }

        var latestEventSummary = summaries.Where(p => p.ResultingClaimState.EqualsIgnoreCase(ClaimState.Declined))
            .OrderBy(p => p.SequenceNumber)
            .LastOrDefault();

        if (latestEventSummary == null)
        {
            return null;
        }

        return latestEventSummary.Timestamp;
    }

    private class ClaimStateChangeEventSummary
    {
        public int SequenceNumber { get; set; }

        public Instant Timestamp { get; set; }

        public string PreviousClaimState { get; set; }

        public string ResultingClaimState { get; set; }
    }
}
