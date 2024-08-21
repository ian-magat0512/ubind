// <copyright file="GetAdditionalPropertyDefinitionByIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query model in getting additional property by id.
    /// </summary>
    public class GetAdditionalPropertyDefinitionByIdQuery : IQuery<AdditionalPropertyDefinitionReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAdditionalPropertyDefinitionByIdQuery"/> class.
        /// </summary>
        /// <param name="id">Primary key of the additional property.</param>
        public GetAdditionalPropertyDefinitionByIdQuery(Guid tenantId, Guid id)
        {
            this.TenantId = tenantId;
            this.Id = id;
        }

        public Guid TenantId { get; }

        public Guid Id { get; }
    }
}
