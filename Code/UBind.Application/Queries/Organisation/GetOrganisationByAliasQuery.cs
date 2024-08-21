// <copyright file="GetOrganisationByAliasQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Gets an organisation by alias.
    /// </summary>
    public class GetOrganisationByAliasQuery : IQuery<OrganisationReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationByAliasQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationAlias">The organisastion Alias.</param>
        public GetOrganisationByAliasQuery(Guid tenantId, string organisationAlias)
        {
            this.TenantId = tenantId;
            this.OrganisationAlias = organisationAlias;
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the organisation Alias.
        /// </summary>
        public string OrganisationAlias { get; }
    }
}
