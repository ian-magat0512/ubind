// <copyright file="GetPersonsMatchingFiltersQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Person
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    ///  A query to return the people records customer by the given person Id.
    /// </summary>
    public class GetPersonsMatchingFiltersQuery : IQuery<IReadOnlyList<IPersonReadModelSummary>>
    {
        public GetPersonsMatchingFiltersQuery(Guid tenantId, EntityListFilters filters)
        {
            this.TenantId = tenantId;
            this.Filters = filters;
        }

        public Guid TenantId { get; private set; }

        public EntityListFilters Filters { get; private set; }
    }
}
