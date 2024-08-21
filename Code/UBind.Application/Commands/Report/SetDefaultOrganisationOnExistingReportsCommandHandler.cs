// <copyright file="SetDefaultOrganisationOnExistingReportsCommandHandler.cs" company="uBind">
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
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command for generating default organisation to existing reports based from tenancy.
    /// </summary>
    public class SetDefaultOrganisationOnExistingReportsCommandHandler
        : ICommandHandler<SetDefaultOrganisationOnExistingReportsCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IReportAggregateRepository reportAggregateRepository;
        private readonly IReportReadModelRepository reportReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationOnExistingReportsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="reportAggregateRepository">The repository for report aggregates.</param>
        /// <param name="reportReadModelRepository">The repository for report read models.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetDefaultOrganisationOnExistingReportsCommandHandler(
            ITenantRepository tenantRepository,
            IReportAggregateRepository reportAggregateRepository,
            IReportReadModelRepository reportReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.reportAggregateRepository = reportAggregateRepository;
            this.reportReadModelRepository = reportReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetDefaultOrganisationOnExistingReportsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var currentTimestamp = this.clock.GetCurrentInstant();
            var tenants = this.tenantRepository.GetTenants();
            var reportsWithNoOrganisation = this.reportReadModelRepository.GetReportsWithNoOrganisationAsQueryable();

            while (true)
            {
                var reportList = reportsWithNoOrganisation.Take(100).ToList();
                if (!reportList.Any())
                {
                    break;
                }

                foreach (var report in reportList)
                {
                    var tenant = tenants.FirstOrDefault(t => t.Id == report.TenantId);
                    var organisation = tenant.Details.DefaultOrganisationId;
                    var reportAggregate = this.reportAggregateRepository.GetById(report.TenantId, report.Id);
                    reportAggregate.RecordOrganisationMigration(organisation, performingUserId, currentTimestamp);
                    await this.reportAggregateRepository.Save(reportAggregate);
                    await Task.Delay(100, cancellationToken);
                }
            }

            return Unit.Value;
        }
    }
}
