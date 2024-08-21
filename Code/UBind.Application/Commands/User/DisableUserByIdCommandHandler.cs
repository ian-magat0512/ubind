// <copyright file="DisableUserByIdCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Handler for disabling user by ID command.
    /// </summary>
    public class DisableUserByIdCommandHandler : ICommandHandler<DisableUserByIdCommand, UserReadModelDetail>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public DisableUserByIdCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            IUserSessionDeletionService userSessionDeletionService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.userSessionDeletionService = userSessionDeletionService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<UserReadModelDetail> Handle(DisableUserByIdCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userAggregate = this.userAggregateRepository.GetById(request.TenantId, request.UserId);
            if (!userAggregate.Blocked)
            {
                userAggregate.Block(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);
                await this.userSessionDeletionService.DeleteForUser(userAggregate.TenantId, userAggregate.Id);
            }

            var user = this.userReadModelRepository.GetUserDetail(request.TenantId, request.UserId);
            return user;
        }
    }
}
