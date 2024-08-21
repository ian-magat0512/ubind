// <copyright file="ReportController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DotLiquid.Exceptions;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Report;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Report;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for report.
    /// </summary>
    [Route("api/v1/report")]
    [RequiresFeature(Feature.Reporting)]
    public class ReportController : PortalBaseController
    {
        private readonly IReportService reportService;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportService">The report service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">Mediator service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public ReportController(
            IReportService reportService,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.reportService = reportService;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Get the reports.
        /// </summary>
        /// <param name="options">The additional query options to be used.</param>
        /// <returns>A response.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReports)]
        [ProducesResponseType(typeof(List<ReportPresentationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReports([FromQuery] ReportQueryOptionsModel options)
        {
            var userTenantId = this.User.GetTenantId();
            await this.GetContextTenantIdOrThrow(options.Tenant, "access a report from a different tenancy");

            PagingHelper.ThrowIfPageNumberInvalid(options?.Page);

            var filters = await options.ToFilters(userTenantId, this.CachingResolver, nameof(ReportReadModel.CreatedTicksSinceEpoch));
            Guid userOrganisationId = this.User.GetOrganisationId();
            await this.authorisationService.ApplyRestrictionsToFilters(this.User, filters);
            List<ReportPresentationModel> reports = this.reportService.GetReportsByTenantIdAndOrganisationId(
                userTenantId, userOrganisationId, userTenantId, filters).ToList();
            return this.Ok(reports);
        }

        /// <summary>
        /// Get the report by Id.
        /// </summary>
        /// <param name="reportId">The report Id.</param>
        /// <param name="tenantId">The tenant ID or Alias.</param>
        /// <returns>A response.</returns>
        [HttpGet]
        [Route("{reportId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReports)]
        [ProducesResponseType(typeof(ReportPresentationModel), StatusCodes.Status200OK)]
        public IActionResult GetReportById(Guid reportId, [FromQuery] Guid? tenantId)
        {
            var userTenantId = this.User.GetTenantId();
            this.authorisationService.ThrowIfUserCannotViewReport(this.User, reportId);
            this.GetContextTenantIdOrThrow(tenantId, "access a report from a different tenancy");

            var report = this.reportService.GetReportById(userTenantId, reportId);
            return this.Ok(report);
        }

        /// <summary>
        /// List previously generated and saved reports.
        /// </summary>
        /// <param name="reportId">The report id.</param>
        /// <param name="options">The tenant ID or Alias.</param>
        /// <returns>A response.</returns>
        [HttpGet("{reportId}/file")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReports)]
        [ProducesResponseType(typeof(IEnumerable<ReportFileSummaryPresentationModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListReportFiles(
            Guid reportId, [FromQuery] QueryOptionsModel options)
        {
            await this.authorisationService.ThrowIfUserCannotViewReport(this.User, reportId);
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "access a report from a different tenancy");
            EntityListFilters filters = await options.ToFilters(tenantId, this.CachingResolver, nameof(ReportFile.CreatedTicksSinceEpoch));
            var environment = string.IsNullOrEmpty(options.Environment) ? "production" : options.Environment;
            var isSuccess = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var reportFiles = await this.reportService.GetReportFiles(reportId, filters);
            return this.Ok(reportFiles);
        }

        /// <summary>
        /// Download a previously generated report file.
        /// </summary>
        /// <param name="reportId">The report id.</param>
        /// <param name="reportFileId">The report file id.</param>
        /// <param name="environment">The environment which the report file data is associated with.</param>
        /// <param name="tenantId">The tenant ID or Alias.</param>
        /// <returns>A response.</returns>
        [HttpGet("{reportId}/file/{reportFileId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewReports)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public IActionResult DownloadReportFile(
            Guid reportId, Guid reportFileId, [FromQuery] string environment, [FromQuery] Guid? tenantId)
        {
            var userTenantId = this.User.GetTenantId();
            this.GetContextTenantIdOrThrow(tenantId, "access a report from a different tenancy");
            this.authorisationService.ThrowIfUserCannotViewReport(this.User, reportId);
            environment = string.IsNullOrEmpty(environment) ? "production" : environment;
            var isSuccess = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var reportFile = this.reportService.GetReportFileFromReport(userTenantId, reportId, reportFileId);
            if (reportFile == null)
            {
                return Errors.General.NotFound("report file", reportFileId).ToProblemJsonResult();
            }

            if (reportFile.Environment != env)
            {
                return Errors.Operations.EnvironmentMisMatch("report").ToProblemJsonResult();
            }

            this.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return this.File(reportFile.Content, reportFile.MimeType, reportFile.Filename);
        }

        /// <summary>
        /// Create the report.
        /// </summary>
        /// <param name="reportModel">The report model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManageReports)]
        [ProducesResponseType(typeof(ReportPresentationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateReport([FromBody] ReportCreateModel reportModel)
        {
            Guid userOrganisationId = this.User.GetOrganisationId();
            var tenantId = this.User.GetTenantId();
            try
            {
                ReportPresentationModel report = await this.reportService.CreateAsync(
                   tenantId,
                   userOrganisationId,
                   reportModel.Name,
                   reportModel.Description,
                   reportModel.ProductIds,
                   reportModel.SourceData,
                   reportModel.MimeType,
                   reportModel.Filename,
                   reportModel.Body,
                   tenantId);

                return this.Ok(report);
            }
            catch (SyntaxException ex)
            {
                return Errors.General.BadRequest(ex.Message).ToProblemJsonResult();
            }
        }

        /// <summary>
        /// Update a existing report.
        /// </summary>
        /// <param name="reportId">The id of the report.</param>
        /// <param name="reportModel">The report model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPut("{reportId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManageReports)]
        [ProducesResponseType(typeof(ReportPresentationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateReport(Guid reportId, [FromBody] ReportCreateModel reportModel)
        {
            await this.authorisationService.ThrowIfUserCannotModifyReport(this.User, reportId);
            try
            {
                ReportPresentationModel report = await this.reportService.UpdateAsync(
                   this.User.GetTenantId(),
                   reportId,
                   reportModel.Name,
                   reportModel.Description,
                   reportModel.ProductIds,
                   reportModel.SourceData,
                   reportModel.MimeType,
                   reportModel.Filename,
                   reportModel.Body,
                   reportModel.IsDeleted);

                return this.Ok(report);
            }
            catch (SyntaxException ex)
            {
                return Errors.General.BadRequest(ex.Message).ToProblemJsonResult();
            }
        }

        /// <summary>
        /// Generate report.
        /// </summary>
        /// <param name="reportId">The report id.</param>
        /// <param name="generateReportDto">The report generation model.</param>
        /// <returns>A report file.</returns>
        [HttpPost("{reportId}/generate")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.GenerateReports)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateReport(
            Guid reportId, [FromBody] GenerateReportDto generateReportDto)
        {
            await this.authorisationService.ThrowIfUserCannotGenerateReport(this.User, reportId);
            var environment = string.IsNullOrEmpty(generateReportDto.Environment)
                ? "production" : generateReportDto.Environment;
            var isSuccess = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            var timeZone = Timezones.GetTimeZoneByIdOrDefault(generateReportDto.TimeZoneId);
            string timeZoneString = timeZone.Id;
            await this.mediator.Send(new GenerateReportCommand(
                this.User.GetTenantId(),
                reportId,
                env,
                generateReportDto.From.GetInstantAtStartOfDayInZone(timeZone),
                generateReportDto.To.GetInstantAtEndOfDayInZone(timeZone),
                timeZoneString,
                generateReportDto.IncludeTestData));
            this.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return this.Ok();
        }
    }
}
