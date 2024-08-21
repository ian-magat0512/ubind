// <copyright file="DeleteAuthenticationMethodCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AuthenticationMethod
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class DeleteAuthenticationMethodCommandHandler : ICommandHandler<DeleteAuthenticationMethodCommand, Unit>
    {
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public DeleteAuthenticationMethodCommandHandler(
            IOrganisationAggregateRepository organisationAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public Task<Unit> Handle(DeleteAuthenticationMethodCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var organisationAggregate
                = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            organisationAggregate.DeleteAuthenticationMethod(command.AuthenticationMethodId, performingUserId, now);
            this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
            return Task.FromResult(Unit.Value);
        }
    }
}
