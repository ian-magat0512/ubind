// <copyright file="AggregateSnapshotService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services;

using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using StackExchange.Profiling;
using System.Text.Json;
using UBind.Application.Json;
using UBind.Application.Services.Email;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Accounting;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Models;
using UBind.Domain.Services;

/// <summary>
/// This service is responsible for adding and retrieving aggregate snapshots.
/// We have a generic type TAggregate which is an aggregate entity.
/// This service is used to serialize and deserialize the aggregate entity.
/// It used by the aggregate repository to save and retrieve the aggregate snapshot.
/// </summary>
/// <typeparam name="TAggregate">The generic type of aggregate entity.</typeparam>
public class AggregateSnapshotService<TAggregate> : IAggregateSnapshotService<TAggregate>
    where TAggregate : class, IAggregateRootEntity<TAggregate, Guid>
{
    private readonly IAggregateSnapshotRepository aggregateSnapshotRepository;
    private readonly IServiceProvider serviceProvider;
    private readonly IClock clock;
    private readonly AggregateJsonContext serializerJsonContext;
    private readonly AggregateJsonContext deserializerJsonContext;

    public AggregateSnapshotService(
        IAggregateSnapshotRepository aggregateSnapshotRepository,
        IServiceProvider serviceProvider,
        IClock clock)
    {
        this.aggregateSnapshotRepository = aggregateSnapshotRepository;
        this.serviceProvider = serviceProvider;
        this.clock = clock;
        this.serializerJsonContext = new AggregateJsonContext(AggregateJsonSerializerSetting.SerializerSetting);
        this.deserializerJsonContext = new AggregateJsonContext(AggregateJsonSerializerSetting.DeserializerSetting);
    }

    public async Task AddAggregateSnapshot(
        Guid tenantId,
        TAggregate aggregate,
        int version)
    {
        async Task AddSnapshot(IServiceProvider serviceProvider)
        {
            // Start a new profiler for this background task
            var profiler = MiniProfiler.StartNew(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.AddAggregateSnapshot));
            try
            {
                using (profiler.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.AddAggregateSnapshot) + "/aggregate type :" + aggregate.AggregateType.ToString()))
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        using (profiler.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.AddAggregateSnapshot) + "/database saving new snapshot"))
                        {
                            string serializedAggregateJson = this.SerializeAggregate(aggregate);
                            var aggregateSnapsnot = new AggregateSnapshot(
                            tenantId,
                            aggregate.Id,
                            aggregate.AggregateType,
                            version,
                            serializedAggregateJson,
                            this.clock.Now());
                            var aggregateSnapshotRepository = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotRepository>();
                            aggregateSnapshotRepository.AddAggregateSnapshot(aggregateSnapsnot);
                            await aggregateSnapshotRepository.SaveChangesAsync();

                            using (profiler.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.AddAggregateSnapshot) + "/delete older snapshots"))
                            {
                                // After saving the new snapshot, we can delete the older snapshots to save disk space.
                                await aggregateSnapshotRepository.DeleteOlderAggregateSnapshots(
                                    tenantId,
                                    aggregateSnapsnot.Id,
                                    aggregateSnapsnot.AggregateId,
                                    aggregateSnapsnot.AggregateType);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var errorNotificationService = scope.ServiceProvider.GetRequiredService<IErrorNotificationService>();
                    errorNotificationService.CaptureSentryException(exception, null);
                }

                throw;
            }
            finally
            {
                // Stop the profiler and handle results as necessary
                profiler?.Stop();
            }
        }

        await RetryPolicyHelper.ExecuteAsync<Exception>(async () => await AddSnapshot(this.serviceProvider));
    }

    public AggregateSnapshotResult<TAggregate>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId, AggregateType aggregateType)
    {
        using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshot)))
        {
            AggregateSnapshot? aggregateSnapshot = null;
            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshot) + "/database query"))
            {
                aggregateSnapshot = this.aggregateSnapshotRepository.GetAggregateSnapshot(tenantId, aggregateId, aggregateType);
                if (aggregateSnapshot == null)
                {
                    return null;
                }
            }

            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshot) + "/deserialize aggregate"))
            {
                TAggregate? aggregate = this.DeserializeAggregate(aggregateSnapshot.Json, aggregateSnapshot.AggregateType);
                if (aggregate == null)
                {
                    return null;
                }
                return new AggregateSnapshotResult<TAggregate>(aggregate, aggregateSnapshot.Version);
            }
        }
    }

    public async Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotAsync(Guid tenantId, Guid aggregateId, AggregateType aggregateType)
    {
        using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshotAsync)))
        {
            AggregateSnapshot? aggregateSnapshot = null;
            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshotAsync) + "/database query"))
            {
                aggregateSnapshot = await this.aggregateSnapshotRepository.GetAggregateSnapshotAsync(tenantId, aggregateId, aggregateType);
                if (aggregateSnapshot == null)
                {
                    return null;
                }
            }

            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshotAsync) + "/deserialize aggregate"))
            {
                TAggregate? aggregate = this.DeserializeAggregate(aggregateSnapshot.Json, aggregateSnapshot.AggregateType);
                if (aggregate == null)
                {
                    return null;
                }
                return new AggregateSnapshotResult<TAggregate>(aggregate, aggregateSnapshot.Version);
            }
        }
    }

    public async Task<AggregateSnapshotResult<TAggregate>?> GetAggregateSnapshotByVersion(Guid tenantId, Guid aggregateId, int version, AggregateType aggregateType)
    {
        using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshot)))
        {
            AggregateSnapshot? aggregateSnapshot = null;
            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshotByVersion) + "/database query"))
            {
                aggregateSnapshot = await this.aggregateSnapshotRepository.GetAggregateSnapshotByVersion(tenantId, aggregateId, version, aggregateType);
                if (aggregateSnapshot == null)
                {
                    return null;
                }
            }

            using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.GetAggregateSnapshotByVersion) + "/deserialize aggregate"))
            {
                TAggregate? aggregate = this.DeserializeAggregate(aggregateSnapshot.Json, aggregateSnapshot.AggregateType);
                if (aggregate == null)
                {
                    return null;
                }
                return new AggregateSnapshotResult<TAggregate>(aggregate, aggregateSnapshot.Version);
            }
        }
    }

    public string SerializeAggregate(TAggregate aggregate)
    {
        using (MiniProfiler.Current.Step(nameof(AggregateSnapshotService<TAggregate>) + "." + nameof(this.SerializeAggregate) + "/serialize aggregate"))
        {
            switch (aggregate.AggregateType)
            {
                case AggregateType.Quote:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.QuoteAggregate);
                case AggregateType.Person:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.PersonAggregate);
                case AggregateType.Customer:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.CustomerAggregate);
                case AggregateType.Claim:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.ClaimAggregate);
                case AggregateType.Organisation:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.Organisation);
                case AggregateType.Report:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.ReportAggregate);
                case AggregateType.TextAdditionalPropertyValue:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.TextAdditionalPropertyValue);
                case AggregateType.AdditionalPropertyDefinition:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.AdditionalPropertyDefinition);
                case AggregateType.FinancialTransaction:
                    return this.SerializeFinancialTransaction(aggregate);
                case AggregateType.Portal:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.PortalAggregate);
                case AggregateType.StructuredDataAdditionalPropertyValue:
                    return JsonSerializer.Serialize(aggregate, this.serializerJsonContext.StructuredDataAdditionalPropertyValue);
                default:
                    throw new NotSupportedException($"Unsupported aggregate type: {aggregate.AggregateType}");
            }
        }
    }

    private string SerializeFinancialTransaction(TAggregate aggregate)
    {
        Type financialTransactionType = typeof(TAggregate);
        switch (financialTransactionType)
        {
            case Type invoiceType when invoiceType == typeof(FinancialTransactionAggregate<Domain.Accounting.Invoice>):
                return JsonSerializer.Serialize(aggregate, AggregateJsonContext.Default.PaymentAggregate);
            case Type creditNoteType when creditNoteType == typeof(FinancialTransactionAggregate<Domain.Accounting.CreditNote>):
                return JsonSerializer.Serialize(aggregate, AggregateJsonContext.Default.RefundAggregate);
            default:
                return string.Empty;
        }
    }

    private TAggregate? DeserializeAggregate(string json, AggregateType aggregateType)
    {
        switch (aggregateType)
        {
            case AggregateType.Quote:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.QuoteAggregate) as TAggregate;
            case AggregateType.Person:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.PersonAggregate) as TAggregate;
            case AggregateType.Customer:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.CustomerAggregate) as TAggregate;
            case AggregateType.Claim:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.ClaimAggregate) as TAggregate;
            case AggregateType.Organisation:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.Organisation) as TAggregate;
            case AggregateType.Report:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.ReportAggregate) as TAggregate;
            case AggregateType.TextAdditionalPropertyValue:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.TextAdditionalPropertyValue) as TAggregate;
            case AggregateType.AdditionalPropertyDefinition:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.AdditionalPropertyDefinition) as TAggregate;
            case AggregateType.FinancialTransaction:
                return this.DeserializeFinancialTransaction(json);
            case AggregateType.Portal:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.PortalAggregate) as TAggregate;
            case AggregateType.StructuredDataAdditionalPropertyValue:
                return JsonSerializer.Deserialize(json, this.deserializerJsonContext.StructuredDataAdditionalPropertyValue) as TAggregate;
            default:
                throw new NotSupportedException($"Unsupported aggregate type: {aggregateType}");
        }
    }

    private TAggregate? DeserializeFinancialTransaction(string json)
    {
        Type financialTransactionType = typeof(TAggregate);
        switch (financialTransactionType)
        {
            case Type invoiceType when invoiceType == typeof(FinancialTransactionAggregate<Domain.Accounting.Invoice>):
                return JsonSerializer.Deserialize(json, AggregateJsonContext.Default.PaymentAggregate) as TAggregate;
            case Type creditNoteType when creditNoteType == typeof(FinancialTransactionAggregate<Domain.Accounting.CreditNote>):
                return JsonSerializer.Deserialize(json, AggregateJsonContext.Default.RefundAggregate) as TAggregate;
            default:
                return null;
        }
    }
}
