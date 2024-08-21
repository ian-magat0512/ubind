// <copyright file="EnableUserByIdCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Handler for enable user by ID command.
    /// </summary>
    public class EnableUserByIdCommandHandler : ICommandHandler<EnableUserByIdCommand, UserReadModelDetail>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public EnableUserByIdCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<UserReadModelDetail> Handle(EnableUserByIdCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userAggregate = this.userAggregateRepository.GetById(request.TenantId, request.UserId);
            if (userAggregate.Blocked)
            {
                userAggregate.Unblock(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.userAggregateRepository.Save(userAggregate);
            }

            var user = this.userReadModelRepository.GetUserDetail(request.TenantId, request.UserId);

            return user;
        }
    }
}
