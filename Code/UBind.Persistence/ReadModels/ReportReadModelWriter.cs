// <copyright file="ReportReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Reponsible for updating the report read model.
    /// </summary>
    public class ReportReadModelWriter : IReportReadModelWriter
    {
        private readonly IWritableReadModelRepository<ReportReadModel> reportReadModelRepository;
        private readonly IProductRepository productRepository;
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportReadModelWriter"/> class.
        /// </summary>
        /// <param name="reportReadModelRepository">The report read model repository.</param>
        /// <param name="productRepository">The product repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        public ReportReadModelWriter(
            IWritableReadModelRepository<ReportReadModel> reportReadModelRepository,
            IProductRepository productRepository,
            ITenantRepository tenantRepository)
        {
            this.reportReadModelRepository = reportReadModelRepository;
            this.productRepository = productRepository;
            this.tenantRepository = tenantRepository;
        }

        public void Dispatch(
            ReportAggregate aggregate,
            IEvent<ReportAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.reportReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var tenant = this.tenantRepository.GetTenantById(@event.TenantId);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var organisationId = @event.OrganisationId == default ?
                defaultOrganisationId : @event.OrganisationId;

            var report = new ReportReadModel(
                tenant.Id,
                organisationId,
                @event.AggregateId,
                @event.Timestamp);
            this.reportReadModelRepository.Add(report);
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportNameUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.Name = @event.Name;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportDescriptionUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.Description = @event.Description;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportProductsUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            var products = this.productRepository.GetActiveProducts();
            products = products.Where(p => p.TenantId == @event.TenantId);

            if (@event.ProductNewIds != null && @event.ProductNewIds.Any())
            {
                products = products.Where(p => @event.ProductNewIds.Contains(p.Id))
                   .ToList();
            }
            else if (@event.ProductIds != null && @event.ProductIds.Any())
            {
                products = products.Where(p => @event.ProductIds.Contains(p.Details.Alias))
                  .ToList();
            }

            report.Products = products.ToList();
            report.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportSourceDataUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.SourceData = @event.SourceData;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportMimeTypeUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.MimeType = @event.MimeType;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportBodyUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.Body = @event.Body;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportFilenameUpdatedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.Filename = @event.Filename;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportDeletedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.LastModifiedTimestamp = @event.Timestamp;
            report.IsDeleted = @event.IsDeleted;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.TenantId = @event.TenantId;
            report.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(ReportAggregate aggregate, ReportAggregate.ReportOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var report = this.GetReportById(@event.TenantId, @event.AggregateId);
            report.OrganisationId = @event.OrganisationId;
            report.LastModifiedTimestamp = @event.Timestamp;
        }

        private ReportReadModel GetReportById(Guid tenantId, Guid reportId)
        {
            return this.reportReadModelRepository
                .GetByIdWithInclude(
                    tenantId,
                    reportId,
                    r => r.Products.Select(p => p.DetailsCollection));
        }
    }
}
