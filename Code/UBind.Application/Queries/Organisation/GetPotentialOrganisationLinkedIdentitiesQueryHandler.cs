// <copyright file="GetPotentialOrganisationLinkedIdentitiesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;

    public class GetPotentialOrganisationLinkedIdentitiesQueryHandler
        : IQueryHandler<GetPotentialOrganisationLinkedIdentitiesQuery, IList<OrganisationLinkedIdentityReadModel>>
    {
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;

        public GetPotentialOrganisationLinkedIdentitiesQueryHandler(
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository)
        {
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
        }

        public Task<IList<OrganisationLinkedIdentityReadModel>> Handle(
            GetPotentialOrganisationLinkedIdentitiesQuery query,
            CancellationToken cancellationToken)
        {
            var organisationIds = new List<Guid>();
            if (query.OrganisationId.HasValue)
            {
                organisationIds.Add(query.OrganisationId.Value);
            }

            if (query.ManagingOrganisationId.HasValue)
            {
                organisationIds.Add(query.ManagingOrganisationId.Value);
            }

            var filters = new EntityListFilters { OrganisationIds = organisationIds };
            var authenticationMethods = this.authenticationMethodReadModelRepository.Get(query.TenantId, filters);
            IList<OrganisationLinkedIdentityReadModel> result = authenticationMethods
                .Where(am => !(am is LocalAccountAuthenticationMethodReadModel))
                .Select(am => new OrganisationLinkedIdentityReadModel
                {
                    TenantId = am.TenantId,
                    OrganisationId = am.OrganisationId,
                    AuthenticationMethodId = am.Id,
                    AuthenticationMethodName = am.Name,
                    AuthenticationMethodTypeName = am.TypeName,
                    UniqueId = string.Empty,
                }).ToList();

            return Task.FromResult(result);
        }
    }
}
