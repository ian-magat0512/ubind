// <copyright file="GenerateReportCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Report
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Report;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;

    /// <summary>
    /// Command handler for generating report then savingg.
    /// </summary>
    public class GenerateReportCommandHandler : ICommandHandler<GenerateReportCommand>
    {
        private readonly IReportAggregateRepository reportAggregateRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IJobClient jobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationOnExistingReportsCommandHandler"/> class.
        /// </summary>
        /// <param name="reportAggregateRepository">The repository for report aggregates.</param>
        /// <param name="jobClient">The enqueque for hangfire.</param>
        public GenerateReportCommandHandler(
            IReportAggregateRepository reportAggregateRepository,
            ICachingResolver cachingResolver,
            IJobClient jobClient)
        {
            this.reportAggregateRepository = reportAggregateRepository;
            this.cachingResolver = cachingResolver;
            this.jobClient = jobClient;
        }

        public Task<Unit> Handle(GenerateReportCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reportDetails = this.reportAggregateRepository.GetById(command.TenantId, command.ReportId);

            if (!(reportDetails.MimeType == "text/plain" || reportDetails.MimeType == "text/csv"))
            {
                throw new ErrorException(Errors.Report.MimeTypeNotSupported(reportDetails.MimeType));
            }

            var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(command.TenantId);
            var jobId = this.jobClient.Enqueue<ReportService>(s =>
                s.GenerateReportFile(command.TenantId, tenantAlias, command.ReportId, command.Environment, command.From, command.To, command.TimeZoneId, command.IncludeTestData));
            return Task.FromResult(Unit.Value);
        }
    }
}
