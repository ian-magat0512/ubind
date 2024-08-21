// <copyright file="ReportEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching report.
    /// This provider support the following searches:
    /// 1. Search by Report Id.
    /// </summary>
    public class ReportEntityProvider : StaticEntityProvider
    {
        private readonly IReportReadModelRepository reportRepository;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The report id.</param>
        /// <param name="reportRepository">The report repository.</param>
        public ReportEntityProvider(IProvider<Data<string>>? id, IReportReadModelRepository reportRepository, ISerialisedEntityFactory serialisedEntityFactory, ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "report")
        {
            this.reportRepository = reportRepository;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Method for retrieving report entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The report entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "reportId",
                    this.SchemaReferenceKey));
            }

            var reportDetail = Guid.TryParse(this.resolvedEntityId, out Guid reportId)
                ? this.reportRepository.SingleOrDefaultIncludeAllProperties(providerContext.AutomationData.ContextManager.Tenant.Id, reportId)
                : default;

            if (reportDetail == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                errorData.Add("reportId", this.resolvedEntityId);
                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Report.Humanize(), "reportId", this.resolvedEntityId, errorData));
            }

            var reportEntity = new Report(reportDetail, providerContext.AutomationData.System.BaseUrl, this.cachingResolver);
            return ProviderResult<Data<IEntity>>.Success(new Data<IEntity>(reportEntity));
        }
    }
}
