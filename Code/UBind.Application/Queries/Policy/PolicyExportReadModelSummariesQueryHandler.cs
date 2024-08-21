// <copyright file="PolicyExportReadModelSummariesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Queries for policy export summaries.
    /// </summary>
    public class PolicyExportReadModelSummariesQueryHandler
        : IQueryHandler<PolicyExportReadModelSummariesQuery, IEnumerable<IPolicyReadModelSummary>>
    {
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ILogger<PolicyExportReadModelSummariesQuery> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyExportReadModelSummariesQueryHandler"/> class.
        /// </summary>
        /// <param name="policyReadModelRepository">The policy read model repo.</param>
        /// <param name="logger">The logger.</param>
        public PolicyExportReadModelSummariesQueryHandler(
            IPolicyReadModelRepository policyReadModelRepository,
            ILogger<PolicyExportReadModelSummariesQuery> logger)
        {
            this.policyReadModelRepository = policyReadModelRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IPolicyReadModelSummary>> Handle(PolicyExportReadModelSummariesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this.GetPoliciesForExport(request));
        }

        private IEnumerable<IPolicyReadModelSummary> GetPoliciesForExport(PolicyExportReadModelSummariesQuery request)
        {
            var summaries = new List<IPolicyReadModelSummary>();
            var pageCount = request.Filters.Page;
            while (true)
            {
                this.logger.LogInformation(
                    string.Format(
                        "Getting Policies Export Batch: {0} records for {1} in Environment: {2}",
                        request.Filters.Page,
                        request.TenantId,
                        Enum.GetName(
                            typeof(DeploymentEnvironment),
                            request.Filters.Environment)));

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                request.Filters.Page = pageCount;
                var policyList = this.policyReadModelRepository.ListPoliciesForExport(request.TenantId, request.Filters);

                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = string.Format(
                    "{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours,
                    ts.Minutes,
                    ts.Seconds,
                    ts.Milliseconds / 10);

                this.logger.LogInformation(string.Format(
                    "It took {0} in getting Policy Export Data Batch: {1} records for {2} in Environment: {3}",
                    elapsedTime,
                    request.Filters.Page,
                    request.TenantId,
                    Enum.GetName(
                        typeof(DeploymentEnvironment),
                        request.Filters.Environment)));

                summaries.AddRange(policyList);

                pageCount++;

                if (policyList?.Count() < 1 ||
                    policyList?.Count() < request.Filters.PageSize ||
                    request.Filters.PageSize < 1)
                {
                    break;
                }
            }

            return summaries;
        }
    }
}
