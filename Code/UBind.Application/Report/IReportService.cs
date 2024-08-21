// <copyright file="IReportService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NodaTime;
using UBind.Domain;
using UBind.Domain.ReadModel;

/// <summary>
/// Service for report creation and update of reports (the report specifications), and report generation.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Create report function.
    /// </summary>
    /// <param name="tenantId">The tenant id of the report.</param>
    /// <param name="organisationId">The organisation id of the report.</param>
    /// <param name="name">The name of the report.</param>
    /// <param name="description">The description of the report.</param>
    /// <param name="producIds">The product ids of the report.</param>
    /// <param name="sourceData">The sourcedata of the report.</param>
    /// <param name="mimeType">The mime-type of the report.</param>
    /// <param name="filename">The filename of the report.</param>
    /// <param name="body">The body of the report.</param>
    /// <param name="performingUserTenantId">The ID of the user requesting the creation.</param>
    /// <returns>A report presentation model.</returns>
    Task<ReportPresentationModel> CreateAsync(
        Guid tenantId,
        Guid organisationId,
        string name,
        string description,
        IEnumerable<Guid> producIds,
        string sourceData,
        string mimeType,
        string filename,
        string body,
        Guid performingUserTenantId);

    /// <summary>
    /// Get all reports.
    /// </summary>
    /// <param name="tenantId">The id of the tenant the report belongs to.</param>
    /// <param name="organisationId">The id of the organisation the report belongs to.</param>
    /// <param name="performingUserTenantId">The ID of the user requesting the query.</param>
    /// <param name="filters">Query filters.</param>
    /// <returns>Enumerable list of report read model.</returns>
    IEnumerable<ReportPresentationModel> GetReportsByTenantIdAndOrganisationId(
        Guid tenantId, Guid organisationId, Guid performingUserTenantId, EntityListFilters filters);

    /// <summary>
    /// Get report by id.
    /// </summary>
    /// <param name="tenantId">The ID of the user requesting the query.</param>
    /// <param name="reportId">The id of the report. </param>
    /// <returns>The report.</returns>
    ReportPresentationModel GetReportById(Guid tenantId, Guid reportId);

    /// <summary>
    /// Gets report files generated from a report definition, created with data in a given environment.
    /// </summary>
    /// <param name="reportId">The report Id.</param>
    /// <param name="filters">Entity list filter to use for filtering on the query.</param>
    /// <returns>A read only list of report files.</returns>
    Task<IReadOnlyList<ReportFileSummaryPresentationModel>> GetReportFiles(
            Guid reportId, EntityListFilters filters);

    /// <summary>
    /// Get report file from report.
    /// </summary>
    /// <param name="tenantId">The ID of the user requesting the query.</param>
    /// <param name="reportId">The id of the report. </param>
    /// <param name="reportFileId">The id of the report file. </param>
    /// <returns>The report file.</returns>
    ReportFilePresentationModel GetReportFileFromReport(
        Guid tenantId, Guid reportId, Guid reportFileId);

    /// <summary>
    /// Update a report.
    /// </summary>
    /// <param name="tenantId">The ID of the user requesting the update.</param>
    /// <param name="reportId">The id of the report.</param>
    /// <param name="name">The name of the report.</param>
    /// <param name="description">The description of the report.</param>
    /// <param name="productIds">The product ids of the report.</param>
    /// <param name="sourceData">The sourcedata of the report.</param>
    /// <param name="mimeType">The mime-type of the report.</param>
    /// <param name="filename">The filename of the report.</param>
    /// <param name="body">The body of the report.</param>
    /// <param name="isDeleted">The value whether the report is deleted.</param>
    /// <returns>A report report presentation model.</returns>
    Task<ReportPresentationModel> UpdateAsync(
        Guid tenantId,
        Guid reportId,
        string name,
        string description,
        IEnumerable<Guid> productIds,
        string sourceData,
        string mimeType,
        string filename,
        string body,
        bool isDeleted);

    /// <summary>
    /// Generate Report File Content.
    /// </summary>
    /// <param name="tenantId">The ID of the user requesting the query.</param>
    /// <param name="reportId">The report id.</param>
    /// <param name="environment">The deployment environment.</param>
    /// <param name="fromTimestmp">The from date of the report.</param>
    /// <param name="toTimestamp">The to date of the report.</param>
    /// <param name="timeZone">The time zone.</param>
    /// <param name="includeTestData">The value whether to include test data.</param>
    [DisplayName("Generate Report File | TENANT: {1}, ENVIRONMENT: {3}, FROM: {4}, TO: {5}, TIMEZONE: {6}, INCLUDETESTDATA: {7}")]
    Task GenerateReportFile(
        Guid tenantId,
        string tenantAlias,
        Guid reportId,
        DeploymentEnvironment environment,
        Instant fromTimestamp,
        Instant toTimestamp,
        string timeZone,
        bool includeTestData);
}
