// <copyright file="GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery : IQuery<AdditionalPropertyDefinitionReadModel>
    {
        public GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery(
            Guid tenantId,
            AdditionalPropertyEntityType entityType,
            string alias)
        {
            this.TenantId = tenantId;
            this.EntityType = entityType;
            this.Alias = alias;
        }

        public Guid TenantId { get; }

        public AdditionalPropertyEntityType EntityType { get; }

        public string Alias { get; }
    }
}
