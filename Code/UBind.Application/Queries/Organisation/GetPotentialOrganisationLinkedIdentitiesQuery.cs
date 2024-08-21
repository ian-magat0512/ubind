// <copyright file="GetPotentialOrganisationLinkedIdentitiesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class GetPotentialOrganisationLinkedIdentitiesQuery : IQuery<IList<OrganisationLinkedIdentityReadModel>>
    {
        public GetPotentialOrganisationLinkedIdentitiesQuery(Guid tenantId, Guid? organisationId, Guid? managingOrganisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ManagingOrganisationId = managingOrganisationId;
        }

        public Guid TenantId { get; }

        public Guid? OrganisationId { get; }

        public Guid? ManagingOrganisationId { get; }
    }
}
