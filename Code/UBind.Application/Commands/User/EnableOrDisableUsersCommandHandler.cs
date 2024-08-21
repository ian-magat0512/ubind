// <copyright file="EnableOrDisableUsersCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Services;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler for enabling or disabling a list of users
    /// </summary>
    public class EnableOrDisableUsersCommandHandler : ICommandHandler<EnableOrDisableUsersCommand, List<UserModel>>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        public EnableOrDisableUsersCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IUserSessionDeletionService userSessionDeletionService,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.userSessionDeletionService = userSessionDeletionService;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public async Task<List<UserModel>> Handle(EnableOrDisableUsersCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.ValidateCommand(command.Users, command);
            var affectedUsers = new List<UserModel>();
            foreach (var user in command.Users)
            {
                var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);
                userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, user.Id, "user");
                var now = this.clock.GetCurrentInstant();
                if (command.Blocked && !userAggregate.Blocked)
                {
                    userAggregate.Block(this.httpContextPropertiesResolver.PerformingUserId, now);
                    await this.userSessionDeletionService.DeleteForUser(userAggregate.TenantId, userAggregate.Id);
                    await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
                    affectedUsers.Add(userAggregate.LatestProjectedReadModel != null
                        ? new UserModel(userAggregate.LatestProjectedReadModel)
                        : user);
                }
                else if (!command.Blocked && userAggregate.Blocked)
                {
                    userAggregate.Unblock(this.httpContextPropertiesResolver.PerformingUserId, now);
                    await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);
                    affectedUsers.Add(userAggregate.LatestProjectedReadModel != null
                        ? new UserModel(userAggregate.LatestProjectedReadModel)
                        : user);
                }
            }

            return affectedUsers;
        }

        private void ValidateCommand(
            IEnumerable<UserModel> users,
            EnableOrDisableUsersCommand command)
        {
            Guid? performingUserTenantId = null;
            Guid? performingUserId = null;
            if (this.httpContextPropertiesResolver.PerformingUser != null)
            {
                performingUserId = this.httpContextPropertiesResolver.PerformingUser.GetId();
                performingUserTenantId = this.httpContextPropertiesResolver.PerformingUser.GetTenantId();
            }

            foreach (var user in users)
            {
                if (command.Blocked && user.TenantId == performingUserTenantId && user.Id == performingUserId)
                {
                    throw new ErrorException(Errors.User.CannotDisableOwnAccount());
                }

                var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);
                if (userAggregate == null)
                {
                    throw new ErrorException(Errors.General.NotFound("User", user.Id));
                }
            }
        }
    }
}
