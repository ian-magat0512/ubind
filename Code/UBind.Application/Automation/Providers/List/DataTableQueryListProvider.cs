// <copyright file="DataTableQueryListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;

    public class DataTableQueryListProvider : IDataListProvider<object>
    {
        private readonly IDataTableDefinitionRepository dataTableDefinitionRepository;
        private readonly IDataTableContentRepository dataTableContentRepository;

        public DataTableQueryListProvider(
            BaseEntityProvider entity,
            IProvider<Data<string>> dataTableAlias,
            IDataTableDefinitionRepository dataTableDefinitionRepository,
            IDataTableContentRepository dataTableContentRepository)
        {
            this.Entity = entity;
            this.DataTableAlias = dataTableAlias;
            this.dataTableDefinitionRepository = dataTableDefinitionRepository;
            this.dataTableContentRepository = dataTableContentRepository;
        }

        public BaseEntityProvider Entity { get; }

        public IProvider<Data<string>> DataTableAlias { get; }

        public List<string> IncludedProperties { get; set; } = new List<string>();

        public string SchemaReferenceKey => "dataTableQueryList";

        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var dataTableAlias = (await this.DataTableAlias.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var entity = (await this.Entity.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var entityType = entity.GetType().Name;
            EntityType parsedEntityType = (EntityType)Enum.Parse(typeof(EntityType), entityType);
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (!Domain.Entities.DataTableDefinition.IsSupportedEntity(parsedEntityType))
            {
                errorData.Add("entityType", entityType);
                errorData.Add("entityId", entity.Id);
                errorData.Add("dataTableAlias", dataTableAlias);

                throw new ErrorException(Domain.Errors.DataTableDefinition
                    .NotSupportedEntity(entityType, errorData));
            }

            var dataTableDefinition = this.dataTableDefinitionRepository
                .GetDataTableDefinitionsByEntityAndAlias(tenantId, entity.Id, parsedEntityType, dataTableAlias);
            if (dataTableDefinition == null)
            {
                errorData.Add("entityType", entityType);
                errorData.Add("entityId", entity.Id);
                errorData.Add("dataTableAlias", dataTableAlias);
                throw new ErrorException(Domain.Errors.DataTableDefinition
                    .AliasNotFound(dataTableAlias, errorData));
            }

            DataTableQueryList<dynamic> result = await this.GetDataQueryList(dataTableDefinition);

            return ProviderResult<IDataList<object>>.Success(result);
        }

        private async Task<DataTableQueryList<dynamic>> GetDataQueryList(Domain.Entities.DataTableDefinition dataTableDefinition)
        {
            using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.GetDataQueryList)}"))
            {
                if (dataTableDefinition.MemoryCachingEnabled)
                {
                    var list = await this.dataTableContentRepository.GetAllDataTableContent(
                        dataTableDefinition.Id,
                        true,
                        dataTableDefinition.CacheExpiryInSeconds);
                    return new DataTableQueryList<dynamic>(list);
                }

                return new DataTableQueryList<dynamic>((top, predicate) =>
                {
                    // this is the callback, data table content will be loaded when the callback is called inside the DataTableQueryList<>.
                    // when you call this you can optionally put in the top, and whereClause parameters that will be appended to the SQL query.
                    return this.dataTableContentRepository.GetDataTableContentWithFilter(
                        dataTableDefinition.Id,
                        top,
                        predicate);
                });
            }
        }
    }
}
