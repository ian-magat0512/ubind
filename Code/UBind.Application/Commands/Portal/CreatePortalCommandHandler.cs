// <copyright file="CreatePortalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Commands.AuthenticationMethod;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    public class CreatePortalCommandHandler : ICommandHandler<CreatePortalCommand, PortalReadModel>
    {
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IPortalReadModelRepository portalReadModelRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly ICqrsMediator cqrsMediator;

        public CreatePortalCommandHandler(
            IPortalAggregateRepository portalAggregateRepository,
            IPortalReadModelRepository portalReadModelRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            ICqrsMediator cqrsMediator)
        {
            this.portalAggregateRepository = portalAggregateRepository;
            this.portalReadModelRepository = portalReadModelRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.cqrsMediator = cqrsMediator;
        }

        public Task<PortalReadModel> Handle(CreatePortalCommand command, CancellationToken cancellationToken)
        {
            if (command.Alias.ToLower() == "null")
            {
                throw new ErrorException(
                    Errors.General.BadRequest($"The alias '{command.Alias}' is invalid"));
            }

            if (this.portalReadModelRepository.PortalNameExistingForTenant(command.TenantId, command.Name))
            {
                throw new ErrorException(Errors.General.DuplicatePropertyValue("portal", "name", command.Name));
            }

            if (this.portalReadModelRepository.PortalAliasExistingForTenant(command.TenantId, command.Alias))
            {
                throw new ErrorException(Errors.General.DuplicatePropertyValue("portal", "alias", command.Alias));
            }

            if (!this.authenticationMethodReadModelRepository.HasLocalAccountRecordForOrganisation(
                command.TenantId, command.OrganisationId))
            {
                // create the "Local Account" authentication method for the organisation
                // it needs to exist so that portal can turn it on or off
                var upsertModel = new LocalAccountAuthenticationMethodUpsertModel
                {
                    Tenant = command.TenantId.ToString(),
                    Organisation = command.OrganisationId.ToString(),
                    Name = "Local Account",
                    TypeName = "Local",
                    IncludeSignInButtonOnPortalLoginPage = true,
                    Disabled = false,
                };
                this.cqrsMediator.Send(new CreateAuthenticationMethodCommand(command.TenantId, command.OrganisationId, upsertModel));
            }

            Guid portalId = Guid.NewGuid();
            var portalAggregate = new PortalAggregate(
                command.TenantId,
                portalId,
                command.Name,
                command.Alias,
                command.Title,
                command.UserType,
                command.OrganisationId,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());
            this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            return Task.FromResult(portalAggregate.ReadModel);
        }
    }
}
