// <copyright file="AdditionalPropertyDefinitionsByModelFilterQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query model in getting the list of <see cref="AdditionalPropertyDefinitionReadModel"/> using a model filter.
    /// </summary>
    public class AdditionalPropertyDefinitionsByModelFilterQuery
        : IQuery<List<AdditionalPropertyDefinitionReadModel>>
    {
        public AdditionalPropertyDefinitionsByModelFilterQuery(
            Guid tenantId,
            AdditionalPropertyDefinitionReadModelFilters additionalPropertyReadModelFilters)
        {
            this.TenantId = tenantId;
            this.Filters = additionalPropertyReadModelFilters;
        }

        public Guid TenantId { get; }

        /// <summary>
        /// Gets the model filters in getting the list of <see cref="AdditionalPropertyDefinitionReadModel"/>.
        /// </summary>
        public AdditionalPropertyDefinitionReadModelFilters Filters { get; }
    }
}
