// <copyright file="GetOrganisationByIdQuery.cs" company="uBind">
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
    /// Gets an organisation.
    /// </summary>
    public class GetOrganisationByIdQuery : IQuery<OrganisationReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationByAliasQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The organisastion ID.</param>
        public GetOrganisationByIdQuery(Guid tenantId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the organisation ID.
        /// </summary>
        public Guid OrganisationId { get; }
    }
}
