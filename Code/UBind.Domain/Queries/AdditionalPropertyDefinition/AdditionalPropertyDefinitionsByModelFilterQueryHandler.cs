// <copyright file="AdditionalPropertyDefinitionsByModelFilterQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query handler for <see cref="AdditionalPropertyDefinitionsByModelFilterQuery"/>.
    /// </summary>
    public class AdditionalPropertyDefinitionsByModelFilterQueryHandler
        : IQueryHandler<AdditionalPropertyDefinitionsByModelFilterQuery,
            List<AdditionalPropertyDefinitionReadModel>>
    {
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionsByModelFilterQueryHandler"/> class.
        /// </summary>
        /// <param name="additionalPropertyDefinitionRepository">Additional property definition repository service.
        /// </param>
        public AdditionalPropertyDefinitionsByModelFilterQueryHandler(
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
        {
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
        }

        /// <inheritdoc/>
        public async Task<List<AdditionalPropertyDefinitionReadModel>> Handle(
            AdditionalPropertyDefinitionsByModelFilterQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.additionalPropertyDefinitionRepository
                .GetByModelFilter(query.TenantId, query.Filters);

            // If the condition is true this will merge the result of the child and parent context
            // Parent context will always be tenant and the child context can be either product or organisation.
            if (query.Filters.MergeResult
                && query.Filters.ParentContextId.HasValue
                && query.Filters.ContextType.HasValue
                && query.Filters.Entity.HasValue
                && query.Filters.ContextId != Guid.Empty)
            {
                query.Filters.ContextType = Enums.AdditionalPropertyDefinitionContextType.Tenant;
                query.Filters.ContextId = query.Filters.ParentContextId.Value;
                query.Filters.ParentContextId = null;
                var parentResult = await this.additionalPropertyDefinitionRepository.GetByModelFilter(query.TenantId, query.Filters);
                if (parentResult != null && parentResult.Any())
                {
                    result = result.Concat(parentResult).OrderBy(ap => ap.Name).ToList();
                }
            }

            return result;
        }
    }
}
