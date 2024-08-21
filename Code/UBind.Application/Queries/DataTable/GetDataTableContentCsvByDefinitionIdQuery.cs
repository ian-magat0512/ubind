// <copyright file="GetDataTableContentCsvByDefinitionIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.DataTable
{
    using System;
    using UBind.Domain.Dto;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Gets the data table content from the database. The information about Database Table where the content was stored
    /// can be find in the Data Table Definition, we will query the Data Table Definition provided with the ID,
    /// then use the DatabaseTableName property to retrieve its content and convert it to CSV.
    /// </summary>
    public class GetDataTableContentCsvByDefinitionIdQuery : IQuery<DataTableContentCsvDto>
    {
        public GetDataTableContentCsvByDefinitionIdQuery(
            Guid tenantId,
            Guid dataTableDefinitionId)
        {
            this.TenantId = tenantId;
            this.DataTableDefinitionId = dataTableDefinitionId;
        }

        public Guid TenantId { get; private set; }

        public Guid DataTableDefinitionId { get; private set; }
    }
}
