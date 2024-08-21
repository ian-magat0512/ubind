// <copyright file="GetDataTableDefinitionByIdQuery.cs" company="uBind">
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
    /// Gets the data table definition by ID.
    /// </summary>
    public class GetDataTableDefinitionByIdQuery : IQuery<DataTableDefinitionDto>
    {
        public GetDataTableDefinitionByIdQuery(Guid tenantId, Guid dataTableDefinitionId)
        {
            this.TenantId = tenantId;
            this.DataTableDefinitionId = dataTableDefinitionId;
        }

        public Guid TenantId { get; private set; }

        public Guid DataTableDefinitionId { get; private set; }
    }
}
