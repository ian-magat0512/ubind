// <copyright file="DisablePortalSignInMethodCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Portal
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class DisablePortalSignInMethodCommandHandler : ICommandHandler<DisablePortalSignInMethodCommand, Unit>
    {
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ICachingResolver cachingResolver;

        public DisablePortalSignInMethodCommandHandler(
            IPortalAggregateRepository portalAggregateRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver)
        {
            this.portalAggregateRepository = portalAggregateRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.cachingResolver = cachingResolver;
        }

        public async Task<Unit> Handle(DisablePortalSignInMethodCommand command, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var portalAggregate = this.portalAggregateRepository.GetById(command.TenantId, command.PortalId);
            portalAggregate.DisableSignInMethod(command.AuthenticationMethodId, performingUserId, now);
            await this.portalAggregateRepository.ApplyChangesToDbContext(portalAggregate);
            this.cachingResolver.RemoveCachedPortals(
                command.TenantId, command.PortalId, new List<string> { portalAggregate.Alias });
            return Unit.Value;
        }
    }
}
