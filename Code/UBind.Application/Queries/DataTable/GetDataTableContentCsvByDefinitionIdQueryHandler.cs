// <copyright file="GetDataTableContentCsvByDefinitionIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.DataTable
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handler for getting the data table content and convert it to CSV.
    /// The information about Database Table where the content was stored
    /// can be find in the Data Table Definition, we will query the Data Table Definition provided with the ID,
    /// then use the DatabaseTableName property to retrieve its content and convert it to CSV.
    /// </summary>
    public class GetDataTableContentCsvByDefinitionIdQueryHandler : IQueryHandler<GetDataTableContentCsvByDefinitionIdQuery, DataTableContentCsvDto>
    {
        private readonly IDataTableContentRepository dataTableContentRepository;
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly ICachingResolver cachingResolver;

        public GetDataTableContentCsvByDefinitionIdQueryHandler(
            IDataTableContentRepository dataTableContentRepository,
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            ICachingResolver cachingResolver)
        {
            this.dataTableContentRepository = dataTableContentRepository;
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.cachingResolver = cachingResolver;
        }

        public async Task<DataTableContentCsvDto> Handle(GetDataTableContentCsvByDefinitionIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(request.TenantId, request.DataTableDefinitionId);
            if (dataTableDefinition == null)
            {
                throw new ErrorException(Errors.DataTableDefinition.NotFound(request.DataTableDefinitionId));
            }

            var dataTableContent = await this.dataTableContentRepository.GetAllDataTableContent(dataTableDefinition.Id, false, 0, null);
            var serializer = DataTableCsvConverter.DataTableDefinitionCsvSerializers;
            var dataTableContentCsv = DataTableCsvConverter.ConvertToCsv(dataTableContent, serializer);
            var downloadFileName = await this.GenerateDataTableContentDownloadFileName(dataTableDefinition);
            return new DataTableContentCsvDto(dataTableDefinition, dataTableContentCsv, downloadFileName);
        }

        /// <summary>
        /// This method will generate the download file name with the given pattern
        /// Example result:
        ///     - Tenant data table:
        ///         data-table.{{ tenantAlias }}.{{ dataTableAlias }}.csv
        ///     - Non-tenant data table:
        ///         data-table.{{ tenantAlias }}.{{ entityAlias }}.{{ dataTableAlias }}.csv.
        /// </summary>
        /// <param name="dataTableDefinition">The data table definition ID.</param>
        /// <returns>The generated download filename.</returns>
        private async Task<string> GenerateDataTableContentDownloadFileName(DataTableDefinition dataTableDefinition)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(dataTableDefinition.TenantId);
            var tenantAlias = tenantModel.Details.Alias;
            var productAlias = string.Empty;
            var organisationAlias = string.Empty;
            if (dataTableDefinition.EntityType == EntityType.Product)
            {
                var productModel = await this.cachingResolver.GetProductOrThrow(dataTableDefinition.TenantId, dataTableDefinition.EntityId);
                productAlias = productModel.Details.Alias;
            }

            if (dataTableDefinition.EntityType == EntityType.Organisation)
            {
                var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(new GuidOrAlias(dataTableDefinition.TenantId), new GuidOrAlias(dataTableDefinition.EntityId));
                organisationAlias = organisationModel.Alias;
            }

            return $"data-table.{tenantAlias}."
                + $"{(!string.IsNullOrEmpty(productAlias) ? productAlias + "." : string.Empty)}"
                + $"{(!string.IsNullOrEmpty(organisationAlias) ? organisationAlias + "." : string.Empty)}"
                + $"{dataTableDefinition.Alias}.csv";
        }
    }
}
