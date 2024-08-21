// <copyright file="SetProductIdsToGuidForReportReadModelCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Report
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Sets product ids of report read models to Guid.
    /// </summary>
    public class SetProductIdsToGuidForReportReadModelCommandHandler
        : ICommandHandler<SetProductIdsToGuidForReportReadModelCommand, Unit>
    {
        private readonly ILogger<SetProductIdsToGuidForReportReadModelCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IReportAggregateRepository reportAggregateRepository;
        private readonly IReportReadModelRepository reportReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetProductIdsToGuidForReportReadModelCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="reportAggregateRepository">The repository for report aggregates.</param>
        /// <param name="reportReadModelRepository">The repository for report read models.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetProductIdsToGuidForReportReadModelCommandHandler(
            ITenantRepository tenantRepository,
            IReportAggregateRepository reportAggregateRepository,
            IReportReadModelRepository reportReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<SetProductIdsToGuidForReportReadModelCommandHandler> logger,
            IClock clock)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.reportAggregateRepository = reportAggregateRepository;
            this.reportReadModelRepository = reportReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetProductIdsToGuidForReportReadModelCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var currentTimestamp = this.clock.GetCurrentInstant();
            var tenants = this.tenantRepository.GetTenants();

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Starting Migration for Tenant {tenant.Details.Alias}.");
                var reports = this.reportReadModelRepository.GetReports(tenant.Id).ToList();

                foreach (var report in reports)
                {
                    var reportAggregate = this.reportAggregateRepository.GetById(report.TenantId, report.Id);
                    this.logger.LogInformation($"Setting Report with Id {report.Id}.");
                    if ((reportAggregate.ProductIds == null || !reportAggregate.ProductIds.Any()) && report.Products.Any())
                    {
                        var productIds = report.Products.Select(x => x.Id).ToList();
                        reportAggregate.UpdateReportProducts(report.TenantId, productIds, performingUserId, currentTimestamp);
                        this.logger.LogInformation($"Saving Report Aggregate with Id {reportAggregate.Id}.");
                        await this.reportAggregateRepository.Save(reportAggregate);
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }

            return Unit.Value;
        }
    }
}
