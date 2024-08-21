// <copyright file="GetOrganisationLinkedIdentitiesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class GetOrganisationLinkedIdentitiesQueryHandler
        : IQueryHandler<GetOrganisationLinkedIdentitiesQuery, IList<OrganisationLinkedIdentityReadModel>>
    {
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        public GetOrganisationLinkedIdentitiesQueryHandler(
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        public async Task<IList<OrganisationLinkedIdentityReadModel>> Handle(GetOrganisationLinkedIdentitiesQuery query, CancellationToken cancellationToken)
        {
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(query.TenantId, query.OrganisationId);
            var linkedIdentities = organisation.LinkedIdentities.ToList();
            if (query.IncludePotential && organisation.ManagingOrganisationId != null) {
                var potentialLinkedIdentities = await this.mediator.Send(
                    new GetPotentialOrganisationLinkedIdentitiesQuery(query.TenantId, organisation.Id, organisation.ManagingOrganisationId));
                foreach (var potential in potentialLinkedIdentities)
                {
                    if (!linkedIdentities.Any(x => x.AuthenticationMethodId == potential.AuthenticationMethodId))
                    {
                        linkedIdentities.Add(potential);
                    }
                }
            }

            return linkedIdentities;
        }
    }
}
