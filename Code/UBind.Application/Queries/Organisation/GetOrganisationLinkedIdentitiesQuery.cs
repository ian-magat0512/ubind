// <copyright file="GetOrganisationLinkedIdentitiesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    /// <summary>
    /// Gets the linked identities for an organisation.
    /// If the includePotential flag is set, then the query will also return empty read models for any potential
    /// linked identities based upon active authentication methods (identity providers) that could apply.
    /// Potential linked identities are used when editing an organisation, to allow the user to add new linked
    /// identities manually.
    /// </summary>
    public class GetOrganisationLinkedIdentitiesQuery : IQuery<IList<OrganisationLinkedIdentityReadModel>>
    {
        public GetOrganisationLinkedIdentitiesQuery(Guid tenantId, Guid organisationId, bool includePotential = false)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.IncludePotential = includePotential;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public bool IncludePotential { get; }
    }
}
