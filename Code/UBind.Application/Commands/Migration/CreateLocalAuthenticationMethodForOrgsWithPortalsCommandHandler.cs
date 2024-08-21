// <copyright file="CreateLocalAuthenticationMethodForOrgsWithPortalsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Commands.AuthenticationMethod;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    public class CreateLocalAuthenticationMethodForOrgsWithPortalsCommandHandler
        : ICommandHandler<CreateLocalAuthenticationMethodForOrgsWithPortalsCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IPortalReadModelRepository portalReadModelRepository;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly ICqrsMediator cqrsMediator;

        public CreateLocalAuthenticationMethodForOrgsWithPortalsCommandHandler(
            ITenantRepository tenantRepository,
            IPortalReadModelRepository portalReadModelRepository,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            ICqrsMediator cqrsMediator)
        {
            this.tenantRepository = tenantRepository;
            this.portalReadModelRepository = portalReadModelRepository;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.cqrsMediator = cqrsMediator;
        }

        public async Task<Unit> Handle(CreateLocalAuthenticationMethodForOrgsWithPortalsCommand request, CancellationToken cancellationToken)
        {
            var tenants = this.tenantRepository.GetTenants(null, false);
            foreach (var tenant in tenants)
            {
                // get all portals for this tenant
                var portals = this.portalReadModelRepository.GetPortals(tenant.Id, new Domain.Filters.PortalListFilters());
                HashSet<Guid> idsOfOrganisationsWithPortals = new HashSet<Guid>();
                foreach (var portal in portals)
                {
                    idsOfOrganisationsWithPortals.Add(portal.OrganisationId);
                }

                foreach (var organisationId in idsOfOrganisationsWithPortals)
                {
                    // check if a the local account authentication method already exists for this org
                    if (!this.authenticationMethodReadModelRepository.HasLocalAccountRecordForOrganisation(
                        tenant.Id, organisationId))
                    {
                        // create the "Local Account" authentication method for the organisation
                        // it needs to exist so that the portal can turn it on or off
                        var upsertModel = new LocalAccountAuthenticationMethodUpsertModel
                        {
                            Tenant = tenant.Id.ToString(),
                            Organisation = organisationId.ToString(),
                            Name = "Local Account",
                            TypeName = "Local",
                            IncludeSignInButtonOnPortalLoginPage = true,
                            Disabled = false,
                        };

                        try
                        {
                            await this.cqrsMediator.Send(new CreateAuthenticationMethodCommand(tenant.Id, organisationId, upsertModel));

                            // delay so we don't overwhelm the database
                            await Task.Delay(2000);
                        }
                        catch (ErrorException ex) when (ex.Error.Code == "organisation.already.has.local.account.auth.method")
                        {
                            // no need to do anything, the authentication method already exists
                        }
                    }
                }
            }

            return Unit.Value;
        }
    }
}
