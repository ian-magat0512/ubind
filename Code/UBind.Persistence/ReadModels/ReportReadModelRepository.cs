// <copyright file="ReportReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;
    using UBind.Persistence.Helpers;

    /// <inheritdoc/>
    public class ReportReadModelRepository : IReportReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public ReportReadModelRepository(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private Func<ReportReadModel, ReportDetailsReadModel> ReportDetailSelector =>
            r => new ReportDetailsReadModel
            {
                Report = r,
                ReportFiles = this.dbContext.ReportFiles
                           .Where(rf => rf.ReportId == r.Id)
                           .Select(this.ReportFileSummarySelector),
            };

        private Func<ReportFile, ReportFileSummary> ReportFileSummarySelector =>
            rf => new ReportFileSummary
            {
                Id = rf.Id,
                CreatedTimestamp = rf.CreatedTimestamp,
                ReportId = rf.ReportId,
                Environment = rf.Environment,
                Filename = rf.Filename,
                Size = rf.Size,
                MimeType = rf.MimeType,
            };

        /// <inheritdoc/>
        public IQueryable<ReportReadModel> GetReportsWithNoOrganisationAsQueryable()
        {
            return this.dbContext.ReportReadModels
                .IncludeAllProperties()
                .Where(r => r.OrganisationId == Guid.Empty);
        }

        /// <inheritdoc/>
        public IQueryable<ReportReadModel> GetReports(Guid tenantId)
        {
            return this.dbContext.ReportReadModels
                .IncludeAllProperties()
                .Where(r => r.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public IEnumerable<ReportDetailsReadModel> GetReportsAndDetailsByTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, EntityListFilters filters)
        {
            var query = this.dbContext.ReportReadModels
                .IncludeAllProperties()
                .Where(r => r.TenantId == tenantId)
                .Where(r => !r.IsDeleted);

            if (organisationId != default)
            {
                query = query.Where(r => r.OrganisationId == organisationId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<ReportReadModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(r => r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<ReportReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<ReportReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Select(this.ReportDetailSelector).Take(1000);
        }

        /// <inheritdoc/>
        public ReportReadModel? SingleOrDefaultIncludeAllProperties(Guid tenantId, Guid reportId)
        {
            return this.dbContext.ReportReadModels
                .IncludeAllProperties()
                .Where(r => r.TenantId == tenantId && r.Id == reportId)
                .Where(r => r.IsDeleted == false)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public ReportDetailsReadModel? GetDetails(Guid tenantId, Guid reportId)
        {
            return this.dbContext.ReportReadModels
                .IncludeAllProperties()
                .Where(r => r.TenantId == tenantId && r.Id == reportId)
                .Where(r => r.IsDeleted == false)
                .Select(this.ReportDetailSelector)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReportFileSummary>> GetReportFiles(Guid reportId, EntityListFilters filters)
        {
            var reportFiles = Enumerable.Empty<ReportFileSummary>();
            var sql = @"SELECT
                Id,
                CreatedTicksSinceEpoch,
                ReportId,
                Environment,
                Filename, 
                Size,
                MimeType
                FROM dbo.ReportFiles
                WHERE ReportId = @ReportId AND Environment = @Environment";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@ReportId", reportId, System.Data.DbType.Guid);
            parameters.Add("@Environment", filters.Environment, System.Data.DbType.Int32);
            sql = SqlQueryPaginator.BuildPaginatedSqlQuery(sql, parameters, filters);
            reportFiles = await this.dbContext.Database.Connection.QueryAsync<ReportFileSummary>(sql, parameters);
            return reportFiles.ToList();
        }

        /// <inheritdoc/>
        public ReportFile? GetReportFile(Guid reportFileId)
        {
            return this.dbContext.ReportFiles.Where(rf => rf.Id == reportFileId).SingleOrDefault();
        }
    }
}
