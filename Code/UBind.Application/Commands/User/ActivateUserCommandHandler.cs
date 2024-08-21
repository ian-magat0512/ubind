// <copyright file="ActivateUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Activates a user by setting the password on their account
    /// </summary>
    public class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand, UserReadModel>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserService userService;

        public ActivateUserCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            IUserService userService)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public async Task<UserReadModel> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userAggregate = this.userAggregateRepository.GetById(command.TenantId, command.UserId);
            if (userAggregate == null || userAggregate.TenantId != command.TenantId)
            {
                throw new ErrorException(Errors.User.Activation.UserNotFound(command.UserId));
            }

            await this.userService.ActivateUser(userAggregate, command.ClearTextPassword);
            return userAggregate.LatestProjectedReadModel;
        }
    }
}
