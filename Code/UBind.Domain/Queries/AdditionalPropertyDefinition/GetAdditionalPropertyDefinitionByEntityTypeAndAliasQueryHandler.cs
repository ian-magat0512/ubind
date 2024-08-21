// <copyright file="GetAdditionalPropertyDefinitionByEntityTypeAndAliasQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetAdditionalPropertyDefinitionByEntityTypeAndAliasQueryHandler
        : IQueryHandler<GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery, AdditionalPropertyDefinitionReadModel>
    {
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;

        public GetAdditionalPropertyDefinitionByEntityTypeAndAliasQueryHandler(
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
        {
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
        }

        public Task<AdditionalPropertyDefinitionReadModel> Handle(GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var additionalPropertyDefinition = this.additionalPropertyDefinitionRepository
                .GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(
                    query.TenantId, query.Alias, query.EntityType);

            return Task.FromResult(additionalPropertyDefinition);
        }
    }
}
