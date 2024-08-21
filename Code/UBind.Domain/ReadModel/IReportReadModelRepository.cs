// <copyright file="IReportReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;

    /// <summary>
    /// The report repository.
    /// </summary>
    public interface IReportReadModelRepository
    {
        /// <summary>
        /// Gets the queryable of reports with no organisations.
        /// </summary>
        /// <returns>A queryable of <see cref="ReportReadModel"/>.</returns>
        IQueryable<ReportReadModel> GetReportsWithNoOrganisationAsQueryable();

        /// <summary>
        /// Gets the queryable of reports under a tenant.
        /// </summary>
        /// <returns>A queryable of <see cref="ReportReadModel"/>.</returns>
        IQueryable<ReportReadModel> GetReports(Guid tenantId);

        /// <summary>
        /// Get reports by tenant Id and organisation Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="filters">Query filters.</param>
        /// <returns>List of reports.</returns>
        IEnumerable<ReportDetailsReadModel> GetReportsAndDetailsByTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, EntityListFilters filters);

        /// <summary>
        /// Get a single report with all the properties.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="reportId">The Id of the report.</param>
        /// <returns>The report.</returns>
        ReportReadModel? SingleOrDefaultIncludeAllProperties(Guid tenantId, Guid reportId);

        /// <summary>
        /// Get reports details by report id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="reportId">The Id of the report.</param>
        /// <returns>The report details.</returns>
        ReportDetailsReadModel? GetDetails(Guid tenantId, Guid reportId);

        /// <summary>
        /// Gets the generated report file summaries for a given report.
        /// </summary>
        /// <param name="reportId">The Id of the report.</param>
        /// <param name="filters">The entity filters to use for filtering queries.</param>
        /// <returns>A list of report files.</returns>
        Task<IReadOnlyList<ReportFileSummary>> GetReportFiles(Guid reportId, EntityListFilters filters);

        /// <summary>
        /// Gets the generated report file for a given report.
        /// </summary>
        /// <param name="reportFileId">The Id of the report file.</param>
        ReportFile? GetReportFile(Guid reportFileId);
    }
}
