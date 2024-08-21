// <copyright file="PaymentAggregateRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates.Accounting;
    using UBind.Domain.Aggregates.Accounting.Payment;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Aggregates;

    /// <summary>
    /// Repository for payment aggregates.
    /// </summary>
    public class PaymentAggregateRepository
        : AggregateRepository<FinancialTransactionAggregate<Invoice>, Guid, EventRecordWithGuidId>, IPaymentAggregateRepository
    {
        private readonly IAggregateSnapshotService<FinancialTransactionAggregate<Invoice>> aggregateSnapshotService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentAggregateRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind DbContext.</param>
        /// <param name="observer">An observer for event dispatching.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="logger">A logger.</param>
        public PaymentAggregateRepository(
            IUBindDbContext dbContext,
            IEventRecordRepository eventRecordRepository,
            IAggregateSnapshotService<FinancialTransactionAggregate<Invoice>> aggregateSnapshotService,
            IFinancialTransactionEventObserver<Invoice> observer,
            IClock clock,
            ILogger<PaymentAggregateRepository> logger,
            IServiceProvider serviceProvider)
            : base(dbContext, eventRecordRepository, events => PaymentAggregate.LoadFromEvents(events), EventRecordWithGuidId.Create, observer, clock, logger)
        {
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.serviceProvider = serviceProvider;
        }

        protected override void InitiateAggregateSnapshotSave(FinancialTransactionAggregate<Invoice> aggregate, int version)
        {
            async Task AddSnapshot(IServiceProvider serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var aggregateSnapshotService = scope.ServiceProvider.GetRequiredService<IAggregateSnapshotService<FinancialTransactionAggregate<Invoice>>>();
                    await aggregateSnapshotService.AddAggregateSnapshot(aggregate.TenantId, aggregate, version);
                }
            }

            // Don't block the main thread for this.
            Task.Run(() => AddSnapshot(this.serviceProvider));
        }

        protected override AggregateSnapshotResult<FinancialTransactionAggregate<Invoice>>? GetAggregateSnapshot(Guid tenantId, Guid aggregateId)
        {
            return this.aggregateSnapshotService.GetAggregateSnapshot(tenantId, aggregateId, AggregateType.FinancialTransaction);
        }

        protected override async Task<AggregateSnapshotResult<FinancialTransactionAggregate<Invoice>>?> GetAggregateSnapshotAsync(
            Guid tenantId,
            Guid aggregateId)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotAsync(
                tenantId,
                aggregateId,
                AggregateType.FinancialTransaction);
        }

        protected override async Task<AggregateSnapshotResult<FinancialTransactionAggregate<Invoice>>?> GetAggregateSnapshotByVersion(
            Guid tenantId,
            Guid aggregateId,
            int version)
        {
            return await this.aggregateSnapshotService.GetAggregateSnapshotByVersion(
                tenantId,
                aggregateId,
                version,
                AggregateType.FinancialTransaction);
        }
    }
}
