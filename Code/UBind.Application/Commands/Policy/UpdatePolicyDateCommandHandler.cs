// <copyright file="UpdatePolicyDateCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy;

using System.Threading;
using Humanizer;
using Newtonsoft.Json.Linq;
using NodaTime;
using ServiceStack;
using UBind.Application.Automation.Extensions;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.DataLocator;
using UBind.Domain.Configuration;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Json;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

/// <summary>
/// This is a command handler to set update a date of a policy
/// This date could be inception date, effective date or expiry date
/// </summary>
public class UpdatePolicyDateCommandHandler : ICommandHandler<UpdatePolicyDateCommand, LocalDateTime>
{
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IEventRecordRepository eventRecordRepository;
    private readonly IProductConfigurationProvider productConfigurationProvider;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService;

    public UpdatePolicyDateCommandHandler(
        IQuoteAggregateRepository quoteAggregateRepository,
        IEventRecordRepository eventRecordRepository,
        IProductConfigurationProvider productConfigurationProvider,
        IReleaseQueryService releaseQueryService,
        IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService)
    {
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.eventRecordRepository = eventRecordRepository;
        this.productConfigurationProvider = productConfigurationProvider;
        this.releaseQueryService = releaseQueryService;
        this.aggregateSnapshotService = aggregateSnapshotService;
    }

    public async Task<LocalDateTime> Handle(UpdatePolicyDateCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var aggregate = await this.quoteAggregateRepository.GetByIdWithoutUsingSnapshot(command.TenantId, command.PolicyId);
        if (aggregate == null)
        {
            throw new ErrorException(Errors.Policy.AggregateNotFound(command.PolicyId));
        }

        var latestQuote = aggregate.GetLatestQuote();
        var timezone = latestQuote.TimeZone;

        var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                aggregate.TenantId,
                aggregate.ProductId,
                aggregate.Environment,
                command.ProductReleaseId);

        var productConfiguration = await this.productConfigurationProvider
            .GetProductConfiguration(releaseContext, WebFormAppType.Quote);

        LocalDateTime newDateTime;
        var (eventSequenceNo, policyUpsertEvent) = this.GetRelevantPolicyUpsertEventWithSequenceNo(command, aggregate);
        switch (command.DateType)
        {
            case PolicyDateType.InceptionDate:
                newDateTime = this.ProcessInceptionDateUpdate(
                    command,
                    aggregate,
                    policyUpsertEvent,
                    eventSequenceNo,
                    productConfiguration,
                    timezone);
                break;
            case PolicyDateType.EffectiveDate:
                newDateTime = this.ProcessEffectiveDateUpdate(
                    command,
                    aggregate,
                    policyUpsertEvent,
                    eventSequenceNo,
                    productConfiguration,
                    timezone);
                break;
            default:
                newDateTime = this.ProcessExpiryDateUpdate(
                    command,
                    aggregate,
                    policyUpsertEvent,
                    eventSequenceNo,
                    productConfiguration,
                    timezone);
                break;
        }

        List<Type> observerTypes = EventObserverHelper.GetObserverTypesForDispatchFlags(true, true, true);
        await this.quoteAggregateRepository.ReplayAllEventsByAggregateId(command.TenantId, aggregate.Id, observerTypes);
        await this.quoteAggregateRepository.Save(aggregate);

        // If the aggregate has more than 200 events, then we'll add a snapshot,
        // otherwise we'll wait until the aggregate has more than 200 events and there a separate process that will add a snapshot.
        if (aggregate.PersistedEventCount >= this.quoteAggregateRepository.GetSnapshotSaveInterval())
        {
            // Add a snapshot after the replaying all the events, since we update the policy dates then load the aggregate after so we can get the updated aggregate to be snapshotted.
            aggregate = await this.quoteAggregateRepository.GetByIdWithoutUsingSnapshot(command.TenantId, aggregate.Id);
            await this.aggregateSnapshotService.AddAggregateSnapshot(command.TenantId, aggregate, aggregate.PersistedEventCount);
        }

        // We'll return the UTC equivalent of the new date time
        return newDateTime.ToTargetTimeZone(timezone, DateTimeZone.Utc);
    }

    private LocalDateTime ProcessInceptionDateUpdate(
        UpdatePolicyDateCommand request,
        QuoteAggregate aggregate,
        IPolicyUpsertEvent policyUpsertEvent,
        int eventSequenceNo,
        IProductConfiguration productConfiguration,
        DateTimeZone timezone)
    {
        var dataLocations = this.GetDataLocations(productConfiguration, PolicyDateType.InceptionDate);

        // Get the new inception date time and timestamp in UTC
        var (dateTime, timestamp) = this.ResolveNewDateTime(request, policyUpsertEvent.EffectiveDateTime, timezone);

        // Apply the new inception date time to the form data
        this.ApplyDateToFormData(request, aggregate, policyUpsertEvent, dataLocations, timestamp);

        // Set the new inception date time and timestamp
        if (policyUpsertEvent is PolicyIssuedEvent policyIssuedEvent)
        {
            policyIssuedEvent.InceptionDateTime = dateTime;
            policyIssuedEvent.InceptionTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyIssuedWithoutQuoteEvent policyIssuedWithoutQuoteEvent)
        {
            policyIssuedWithoutQuoteEvent.InceptionDateTime = dateTime;
            policyIssuedWithoutQuoteEvent.InceptionTimestamp = timestamp;
        }

        this.eventRecordRepository.UpdateEventRecord(
                aggregate.TenantId,
                aggregate.Id,
                aggregate.AggregateType,
                eventSequenceNo,
                policyUpsertEvent);
        this.eventRecordRepository.SaveChanges();
        return dateTime;
    }

    private LocalDateTime ProcessEffectiveDateUpdate(
        UpdatePolicyDateCommand request,
        QuoteAggregate aggregate,
        IPolicyUpsertEvent policyUpsertEvent,
        int eventSequenceNo,
        IProductConfiguration productConfiguration,
        DateTimeZone timezone)
    {
        // Get the new effective date time and timestamp in UTC
        var (dateTime, timestamp) = this.ResolveNewDateTime(request, policyUpsertEvent.EffectiveDateTime, timezone);

        try
        {
            // Apply the new effective date time to the form data
            var dataLocations = this.GetDataLocations(productConfiguration, PolicyDateType.EffectiveDate);
            this.ApplyDateToFormData(request, aggregate, policyUpsertEvent, dataLocations, timestamp);
        }
        catch (ErrorException e)
        {
            // Fall back of the policy upsert events that has no effective date in form data and calculation result
            var upsertEventsWithNoEffectiveDatePossibility = new List<Type>
            {
                typeof(PolicyIssuedEvent),
                typeof(PolicyIssuedWithoutQuoteEvent),
                typeof(PolicyRenewedEvent),
                typeof(PolicyImportedEvent),
            };
            if (e.Error.Code == "policy.date.patching.property.not.found"
                && upsertEventsWithNoEffectiveDatePossibility.Contains(policyUpsertEvent.GetType()))
            {
                // If the upsert event does not have effective date in the form data and calculation result,
                // then we need to apply the new effective date time to the inception date data locations
                var inceptionDataLocations = this.GetDataLocations(productConfiguration, PolicyDateType.InceptionDate);
                this.ApplyDateToFormData(request, aggregate, policyUpsertEvent, inceptionDataLocations, timestamp);
            }
            else
            {
                e.EnrichAndRethrow(null);
                throw;
            }
        }

        // Set the new effective date time and timestamp,
        if (policyUpsertEvent is PolicyIssuedEvent policyIssuedEvent)
        {
            policyIssuedEvent.InceptionDateTime = dateTime;
            policyIssuedEvent.InceptionTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyIssuedWithoutQuoteEvent policyIssuedWithoutQuoteEvent)
        {
            policyIssuedWithoutQuoteEvent.InceptionDateTime = dateTime;
            policyIssuedWithoutQuoteEvent.InceptionTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyAdjustedEvent policyAdjustedEvent)
        {
            policyAdjustedEvent.EffectiveDateTime = dateTime;
            policyAdjustedEvent.EffectiveTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyRenewedEvent policyRenewedEvent)
        {
            policyRenewedEvent.EffectiveDateTime = dateTime;
            policyRenewedEvent.EffectiveTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyCancelledEvent policyCancelledEvent)
        {
            policyCancelledEvent.EffectiveDateTime = dateTime;
            policyCancelledEvent.EffectiveTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyImportedEvent policyImported)
        {
            policyImported.InceptionDateTime = dateTime;
            policyImported.InceptionTimestamp = timestamp;
        }

        this.eventRecordRepository.UpdateEventRecord(
                aggregate.TenantId,
                aggregate.Id,
                aggregate.AggregateType,
                eventSequenceNo,
                policyUpsertEvent);
        this.eventRecordRepository.SaveChanges();
        return dateTime;
    }

    private LocalDateTime ProcessExpiryDateUpdate(
        UpdatePolicyDateCommand request,
        QuoteAggregate aggregate,
        IPolicyUpsertEvent policyUpsertEvent,
        int eventSequenceNo,
        IProductConfiguration productConfiguration,
        DateTimeZone timezone)
    {
        var dataLocations = this.GetDataLocations(productConfiguration, PolicyDateType.ExpiryDate);

        // Get the new expiry date time and timestamp in UTC
        var (dateTime, timestamp) = this.ResolveNewDateTime(request, policyUpsertEvent.ExpiryDateTime.Value, timezone);

        // Apply the new expiry date time to the form data
        this.ApplyDateToFormData(request, aggregate, policyUpsertEvent, dataLocations, timestamp);

        // Set the new expiry date time and timestamp,
        // then re-set the effective timestamp in UTC (this has to be done otherwise the wrong effective date time will be displayed)
        if (policyUpsertEvent is PolicyIssuedEvent policyIssuedEvent)
        {
            policyIssuedEvent.ExpiryDateTime = dateTime;
            policyIssuedEvent.ExpiryTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyIssuedWithoutQuoteEvent policyIssuedWithoutQuoteEvent)
        {
            policyIssuedWithoutQuoteEvent.ExpiryDateTime = dateTime;
            policyIssuedWithoutQuoteEvent.ExpiryTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyAdjustedEvent policyAdjustedEvent)
        {
            policyAdjustedEvent.ExpiryDateTime = dateTime;
            policyAdjustedEvent.ExpiryTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyRenewedEvent policyRenewedEvent)
        {
            policyRenewedEvent.ExpiryDateTime = dateTime;
            policyRenewedEvent.ExpiryTimestamp = timestamp;
        }
        else if (policyUpsertEvent is PolicyCancelledEvent)
        {
            throw new ErrorException(Errors.Policy.DatePatching.CannotPatchExpiryDateOfCancelledPolicy(
                request.PolicyId.ToString(),
                this.CreateErrorData(aggregate)));
        }
        else if (policyUpsertEvent is PolicyImportedEvent policyImported)
        {
            policyImported.ExpiryDateTime = dateTime;
            policyImported.ExpiryTimestamp = timestamp;
        }

        this.eventRecordRepository.UpdateEventRecord(
                aggregate.TenantId,
                aggregate.Id,
                aggregate.AggregateType,
                eventSequenceNo,
                policyUpsertEvent);
        this.eventRecordRepository.SaveChanges();
        return dateTime;
    }

    private (int, IPolicyUpsertEvent) GetRelevantPolicyUpsertEventWithSequenceNo(
        UpdatePolicyDateCommand request,
        QuoteAggregate aggregate)
    {
        var dateType = request.DateType;

        // If the date type is inception date, then we need to get the earliest policy upsert event which
        // should either be PolicyIssuedEvent or PolicyIssuedWithoutQuoteEvent
        if (dateType == PolicyDateType.InceptionDate)
        {
            var events = this.GetPolicyUpsertEventsWithSequenceNo(aggregate, dateType);
            var validEvents = events.Where(a => a.Value is PolicyIssuedEvent || a.Value is PolicyIssuedWithoutQuoteEvent);
            if (!validEvents.Any())
            {
                // If there is no PolicyIssuedEvent or PolicyIssuedWithoutQuoteEvent, then we cannot patch inception date
                throw new ErrorException(Errors.Policy.DatePatching.RequiredUpsertEventForInceptionDatePatchingNotFound(
                    request.PolicyId.ToString(),
                    this.CreateErrorData(aggregate)));
            }

            var eventWithSequenceNo = validEvents.MinBy(a => a.Value.Timestamp);
            return (eventWithSequenceNo.Key, eventWithSequenceNo.Value);
        }

        // If the date type is effective date or expiry date, then we need to get the latest policy upsert event
        return this.GetLatestUpsertEventWithSequenceNo(aggregate, dateType);
    }

    private (int, IPolicyUpsertEvent) GetLatestUpsertEventWithSequenceNo(
        QuoteAggregate aggregate,
        PolicyDateType dateType)
    {
        // Get the latest policy upsert event
        var events = this.GetPolicyUpsertEventsWithSequenceNo(aggregate, PolicyDateType.EffectiveDate);
        try
        {
            var upsertEvent = events.OfType<KeyValuePair<int, IPolicyUpsertEvent>>().MaxBy(a => a.Value.Timestamp);
            return (upsertEvent.Key, upsertEvent.Value);
        }
        catch (InvalidOperationException)
        {
            var errorData = this.CreateErrorData(aggregate);
            throw new ErrorException(Errors.Policy.DatePatching.NoPolicyUpsertEventFound(
                aggregate.Policy?.PolicyId.ToString(),
                dateType.Humanize(),
                errorData));
        }
    }

    private IEnumerable<KeyValuePair<int, IPolicyUpsertEvent>> GetPolicyUpsertEventsWithSequenceNo(
               QuoteAggregate aggregate, PolicyDateType dateType)
    {
        var events = aggregate.GetRemainingEventsWithSequenceNumberAfterRollbacks().Where(a => a.Value is IPolicyUpsertEvent);
        if (!events.Any())
        {
            var errorData = this.CreateErrorData(aggregate);
            throw new ErrorException(Errors.Policy.DatePatching.NoPolicyUpsertEventFound(
                aggregate.Policy?.PolicyId.ToString(),
                dateType.Humanize(),
                errorData));
        }

        return events
            .Select(a => new KeyValuePair<int, IPolicyUpsertEvent>(a.Key, (IPolicyUpsertEvent)a.Value))
            .ToList();
    }

    private List<DataLocation> GetDataLocations(IProductConfiguration productConfiguration, PolicyDateType dateType)
    {
        var dataLocators = productConfiguration.DataLocators;
        var defaultDataLocators = DefaultDataLocations.Instance;
        var quoteDataLocations = productConfiguration.QuoteDataLocations;
        var defaultQuoteDataLocations = DefaultQuoteDatumLocations.Instance;
        List<DataLocation> dataLocations = new List<DataLocation>();
        QuoteDatumLocation? quoteDatumLocation = null;
        switch (dateType)
        {
            case PolicyDateType.InceptionDate:
                dataLocations.AddRange(dataLocators?.InceptionDate ?? defaultDataLocators.InceptionDate);
                quoteDatumLocation = quoteDataLocations?.InceptionDate ?? defaultQuoteDataLocations.InceptionDate;
                break;
            case PolicyDateType.EffectiveDate:
                dataLocations.AddRange(dataLocators?.EffectiveDate ?? defaultDataLocators.EffectiveDate);
                quoteDatumLocation = quoteDataLocations?.EffectiveDate ?? defaultQuoteDataLocations.EffectiveDate;
                break;
            default:
                dataLocations.AddRange(dataLocators?.ExpiryDate ?? defaultDataLocators.ExpiryDate);
                quoteDatumLocation = quoteDataLocations?.ExpiryDate ?? defaultQuoteDataLocations.ExpiryDate;
                break;
        }

        var location = new DataLocation(
            quoteDatumLocation.Object == QuoteDataLocationObject.FormData
                ? DataSource.FormData
                : DataSource.Calculation,
            quoteDatumLocation.Path);
        if (!dataLocations.Any(a => a.Path == location.Path && a.Source == location.Source))
        {
            dataLocations.Add(location);
        }

        return dataLocations;
    }

    private void ApplyDateToFormData(
        UpdatePolicyDateCommand request,
        QuoteAggregate aggregate,
        IPolicyUpsertEvent policyUpsertEvent,
        List<DataLocation> dataLocations,
        Instant timestamp)
    {
        var date = timestamp.ToDdMmYyyyString();
        var hasPatchedAny = false;
        var formData = policyUpsertEvent.DataSnapshot.FormData.Data;
        var calculationResult = policyUpsertEvent.DataSnapshot.CalculationResult.Data;
        foreach (var location in dataLocations)
        {
            if (this.PatchValueToAllMatchingProperties(formData.FormModel, location.Path, date))
            {
                hasPatchedAny = true;
            }
            if (this.PatchValueToAllMatchingProperties(calculationResult.JObject, location.Path, date))
            {
                hasPatchedAny = true;
            }
        }

        if (!hasPatchedAny)
        {
            var errorData = this.CreateErrorData(aggregate);
            errorData.Add("dataLocations", dataLocations.ToJson());
            throw new ErrorException(Errors.Policy.DatePatching.PropertyNotFound(
                request.PolicyId.ToString(), request.DateType.Humanize(), errorData));
        }
    }

    private bool PatchValueToAllMatchingProperties(
        JObject jObject,
        string path,
        string value)
    {
        bool hasPatchedAny = false;
        var patchRules = PatchRules.PropertyExists;
        JsonPath jsonPath = new JsonPath(path);
        if (jObject.CanPatchProperty(jsonPath, patchRules).IsSuccess)
        {
            jObject.PatchProperty(jsonPath, value);
            hasPatchedAny = true;
        }

        foreach (var property in jObject.Properties())
        {
            if (property.Value is JObject childObject)
            {
                if (this.PatchValueToAllMatchingProperties(childObject, path, value))
                {
                    hasPatchedAny = true;
                }
            }
        }

        return hasPatchedAny;
    }

    private (LocalDateTime, Instant) ResolveNewDateTime(
        UpdatePolicyDateCommand request,
        LocalDateTime currentDateTime,
        DateTimeZone timezone)
    {
        var time = request.Time;
        if (!time.HasValue)
        {
            time = currentDateTime.TimeOfDay;
        }

        var dateTime = request.Date.At(time.Value);
        var timestamp = dateTime.InZoneLeniently(timezone).ToInstant();
        return (dateTime, timestamp);
    }

    private JObject CreateErrorData(QuoteAggregate aggregate)
    {
        return new JObject
        {
            { "tenantId", aggregate.TenantId },
            { "productId", aggregate.ProductId },
            { "policyId", aggregate.Policy?.PolicyId },
            { "aggregateId", aggregate.Id },
        };
    }
}
