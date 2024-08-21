// <copyright file="CreateAuthenticationMethodCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AuthenticationMethod
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Services;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class CreateAuthenticationMethodCommandHandler
        : ICommandHandler<CreateAuthenticationMethodCommand, AuthenticationMethodReadModelSummary>
    {
        private readonly IAuthenticationMethodService authenticationMethodService;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;

        public CreateAuthenticationMethodCommandHandler(
            IOrganisationAggregateRepository organisationAggregateRepository,
            IAuthenticationMethodService authenticationMethodService)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.authenticationMethodService = authenticationMethodService;
        }

        public async Task<AuthenticationMethodReadModelSummary> Handle(
            CreateAuthenticationMethodCommand command, CancellationToken cancellationToken)
        {
            var organisationAggregate
                = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            EntityHelper.ThrowIfNotFound(organisationAggregate, command.OrganisationId, "Organisation");
            organisationAggregate
                = await this.authenticationMethodService.Create(organisationAggregate, command.AuthenticationMethod);
            return organisationAggregate.LatestProjectedAuthenticationMethodReadModel;
        }
    }
}
