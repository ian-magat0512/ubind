// <copyright file="UnlinkIdentityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class UnlinkIdentityCommandHandler : ICommandHandler<UnlinkIdentityCommand, Unit>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        public UnlinkIdentityCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        public Task<Unit> Handle(UnlinkIdentityCommand command, CancellationToken cancellationToken)
        {
            Guid? performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            Instant now = this.clock.Now();
            var userAggregate = this.userAggregateRepository.GetById(command.TenantId, command.UserId);
            userAggregate.UnlinkIdentity(command.AuthenticationMethodId, performingUserId, now);
            this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
            return Task.FromResult(Unit.Value);
        }
    }
}
