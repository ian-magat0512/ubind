// <copyright file="ReportService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using DotLiquid;
    using Humanizer;
    using MoreLinq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Report;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Service for report.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IDropGenerationService dropGenerationService;
        private readonly IReportAggregateRepository reportAggregateRepository;
        private readonly IClock clock;
        private readonly IReportReadModelRepository reportReadModelRepository;
        private readonly IReportFileRepository reportFileRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="reportAggregateRepository">The report aggregate repository.</param>
        /// <param name="clock">The clock for obtaining current time.</param>
        /// <param name="reportReadModelRepository">The repository of the report.</param>
        /// <param name="reportFileRepository">The report file repository.</param>
        /// <param name="dropGenerationService">The drop generation service.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ReportService(
            IReportAggregateRepository reportAggregateRepository,
            IClock clock,
            IReportReadModelRepository reportReadModelRepository,
            IReportFileRepository reportFileRepository,
            IDropGenerationService dropGenerationService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.dropGenerationService = dropGenerationService;
            this.reportAggregateRepository = reportAggregateRepository;
            this.clock = clock;
            this.reportReadModelRepository = reportReadModelRepository;
            this.reportFileRepository = reportFileRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public async Task<ReportPresentationModel> CreateAsync(
            Guid tenantId,
            Guid organisationId,
            string name,
            string description,
            IEnumerable<Guid> productIds,
            string sourceData,
            string mimeType,
            string filename,
            string body,
            Guid performingUserTenantId)
        {
            this.ThrowIfUserUnauthorised(performingUserTenantId, tenantId, organisationId);

            var report = new ReportAddModel
            {
                Name = name,
                Description = description,
                ProductIds = productIds.ToList(),
                SourceData = sourceData,
                MimeType = mimeType,
                Filename = filename,
                Body = body,
            };

            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var reportAggregate = ReportAggregate.CreateReport(tenantId, organisationId, report, performingUserId, this.clock.Now());
            await this.reportAggregateRepository.Save(reportAggregate);
            var reportDetails = this.reportReadModelRepository.GetDetails(tenantId, reportAggregate.Id);
            if (reportDetails == null)
            {
                throw new ErrorException(Errors.Report.NotFound(reportAggregate.Id));
            }

            return new ReportPresentationModel(reportDetails);
        }

        /// <inheritdoc/>
        public IEnumerable<ReportPresentationModel> GetReportsByTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, Guid performingUserTenantId, EntityListFilters filters)
        {
            this.ThrowIfUserUnauthorised(performingUserTenantId, tenantId, organisationId);

            var reportDetails = this.reportReadModelRepository
                .GetReportsAndDetailsByTenantIdAndOrganisationId(tenantId, organisationId, filters)
                .Select(r => new ReportPresentationModel(r));
            return reportDetails;
        }

        /// <inheritdoc/>
        public ReportPresentationModel GetReportById(Guid tenantId, Guid reportId)
        {
            var reportDetails = this.reportReadModelRepository.GetDetails(tenantId, reportId);
            if (reportDetails == null)
            {
                throw new ErrorException(Errors.Report.DetailsNotFound(reportId));
            }

            var reportTenantId = reportDetails.Report.TenantId;
            var reportOrganisationId = reportDetails.Report.OrganisationId;
            this.ThrowIfUserUnauthorised(tenantId, reportTenantId, reportOrganisationId);
            return new ReportPresentationModel(reportDetails);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReportFileSummaryPresentationModel>> GetReportFiles(
            Guid reportId, EntityListFilters filters)
        {
            return (await this.reportReadModelRepository.GetReportFiles(reportId, filters))
                .Select(rf => new ReportFileSummaryPresentationModel(rf)).ToList();
        }

        /// <inheritdoc/>
        public ReportFilePresentationModel GetReportFileFromReport(
            Guid tenantId, Guid reportId, Guid reportFileId)
        {
            var reportDetails = this.reportReadModelRepository.GetDetails(tenantId, reportId);
            if (reportDetails == null)
            {
                throw new ErrorException(Errors.Report.NotFound(reportId));
            }
            var reportTenantId = reportDetails.Report.TenantId;
            var reportOrganisationId = reportDetails.Report.OrganisationId;
            this.ThrowIfUserUnauthorised(tenantId, reportTenantId, reportOrganisationId);
            var reportFile = this.reportReadModelRepository.GetReportFile(reportFileId);
            if (reportFile == null)
            {
                throw new ErrorException(Errors.Report.FileNotFound(reportFileId));
            }

            return new ReportFilePresentationModel(reportFile);
        }

        /// <inheritdoc/>
        public async Task<ReportPresentationModel> UpdateAsync(
            Guid tenantId,
            Guid reportId,
            string name,
            string description,
            IEnumerable<Guid> productIds,
            string sourceData,
            string mimeType,
            string filename,
            string body,
            bool isDeleted)
        {
            var parsedBody = this.ParseLiquidTemplate(body);
            var parsedFilename = this.ParseLiquidTemplate(filename);

            var reportAggregate = this.reportAggregateRepository.GetById(tenantId, reportId);
            var reportDetails = this.reportReadModelRepository.GetDetails(tenantId, reportId);

            if (reportAggregate == null || reportDetails == null)
            {
                throw new ErrorException(Errors.Report.NotFound(reportId));
            }

            this.ThrowIfUserUnauthorised(tenantId, reportAggregate.TenantId, reportAggregate.OrganisationId);

            async Task UpdateAndSaveAsync()
            {
                if (name != reportAggregate.Name)
                {
                    reportAggregate.UpdateReportName(name, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (description != reportAggregate.Description)
                {
                    reportAggregate.UpdateReportDescription(description, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (!productIds.OrderBy(id => id).SequenceEqual(reportAggregate.ProductIds.OrderBy(id => id)))
                {
                    reportAggregate.UpdateReportProducts(tenantId, productIds, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (sourceData != reportAggregate.SourceData)
                {
                    reportAggregate.UpdateReportSourceData(sourceData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (mimeType != reportAggregate.MimeType)
                {
                    reportAggregate.UpdateReportMimeType(mimeType, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (filename != reportAggregate.Filename)
                {
                    reportAggregate.UpdateReportFilename(filename, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (body != reportAggregate.Body)
                {
                    reportAggregate.UpdateReportBody(body, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }

                if (isDeleted != reportAggregate.IsDeleted)
                {
                    reportAggregate.DeleteReport(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                    reportDetails.Report.IsDeleted = isDeleted;
                }

                await this.reportAggregateRepository.Save(reportAggregate);
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                UpdateAndSaveAsync,
                () => reportAggregate = this.reportAggregateRepository.GetById(tenantId, reportId));

            if (!reportAggregate.IsDeleted)
            {
                reportDetails = this.reportReadModelRepository.GetDetails(tenantId, reportId);
            }

            if (reportDetails == null)
            {
                throw new ErrorException(Errors.Report.NotFound(reportId));
            }

            return new ReportPresentationModel(reportDetails);
        }

        /// <inheritdoc/>
        [DisplayName("Generate Report File | TENANT: {1}, ENVIRONMENT: {3}, FROM: {4}, TO: {5}, TIMEZONE: {6}, INCLUDETESTDATA: {7}")]
        public async Task GenerateReportFile(
            Guid tenantId,
            string tenantAlias,
            Guid reportId,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            string timeZone,
            bool includeTestData)
        {
            var reportDetails = this.reportAggregateRepository.GetById(tenantId, reportId);
            reportDetails = EntityHelper.ThrowIfNotFound(reportDetails, reportId, "report");

            var sourceData = reportDetails.SourceData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Dehumanize().ToEnumOrThrow<ReportSourceDataType>());

            DateTimeZone timeZoneId = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone) ?? Timezones.AET;
            var reportDrop = await this.dropGenerationService.GenerateReportDrop(
                reportDetails.TenantId,
                reportDetails.OrganisationId,
                reportDetails.ProductIds,
                environment,
                sourceData,
                fromTimestamp,
                toTimestamp,
                includeTestData);

            var fileName = this.GenerateFilename(
                reportDetails.Name,
                reportDetails.Description,
                reportDetails.MimeType,
                fromTimestamp,
                toTimestamp,
                reportDetails.Filename,
                timeZoneId);
            var parsedReportBody = this.ParseLiquidTemplate(reportDetails.Body);
            var renderedReportBody = this.RenderLiquidTemplate(parsedReportBody, reportDrop);

            byte[] fileContent = new UTF8Encoding().GetBytes(renderedReportBody);

            await this.SaveGeneratedReportAsync(tenantId, reportId, environment, fileName, fileContent, reportDetails.MimeType);
        }

        private string GenerateFilename(
            string name,
            string description,
            string mimeType,
            Instant fromTimestamp,
            Instant toTimestamp,
            string templateSource,
            DateTimeZone timeZone)
        {
            var report = new FileNameDetailsViewModel
            {
                Name = name,
                Description = description,
                MimeType = mimeType,
                FromDate = fromTimestamp.ToLocalDateTimeInZone(timeZone).ToShortDateFormat(),
                ToDate = toTimestamp.ToLocalDateTimeInZone(timeZone).ToShortDateFormat(),
                GeneratedDate = this.clock.Now().ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-AU")),
            };

            var reportTemplateSource = new ReportFileNameViewModel(report);
            var parsedFilename = this.ParseLiquidTemplate(templateSource);
            return this.RenderLiquidTemplate(parsedFilename, reportTemplateSource);
        }

        private async Task SaveGeneratedReportAsync(
            Guid tenantId,
            Guid reportId,
            DeploymentEnvironment environment,
            string fileName,
            byte[] fileContent,
            string mimeType)
        {
            var reportAggregate = this.reportAggregateRepository.GetById(tenantId, reportId);

            if (reportAggregate == null)
            {
                throw new ErrorException(Errors.Report.NotFound(reportId));
            }

            var reportTenantId = reportAggregate.TenantId;
            var reportOrganisationId = reportAggregate.OrganisationId;
            this.ThrowIfUserUnauthorised(tenantId, reportTenantId, reportOrganisationId);

            Guid reportFileId = Guid.NewGuid();
            var reportFile = new ReportFile(
                reportFileId, reportAggregate.Id, environment, fileName, fileContent, mimeType, this.clock.Now());

            async Task SaveAsync()
            {
                var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                reportAggregate.AddGeneratedReportFile(reportFileId, performingUserId, this.clock.Now());
                this.reportFileRepository.Insert(reportFile);
                await this.reportAggregateRepository.Save(reportAggregate);
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(SaveAsync);
        }

        private Template ParseLiquidTemplate(string liquidTemplateSource)
        {
            Template? template = null;

            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            template = Template.Parse(liquidTemplateSource);

            return template;
        }

        private string RenderLiquidTemplate(Template template, Drop drop)
        {
            var hash = Hash.FromAnonymousObject(drop);
            return template.Render(hash);
        }

        private async void ThrowIfUserUnauthorised(
            Guid userRequestingTenantId, Guid resourceTenantId, Guid resourceOrganisationId)
        {
            if (userRequestingTenantId != resourceTenantId)
            {
                var tenant = await this.cachingResolver.GetTenantOrThrow(resourceTenantId);
                throw new NotAuthorisedException<string>(
                    resourceTenantId.ToString(), $"User is not authorised to access report from tenant with ID '{resourceTenantId.ToString()}' and alias '{tenant.Details.Alias}'.");
            }
        }
    }
}
