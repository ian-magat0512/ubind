// <copyright file="CreateUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserReadModel>
    {
        private readonly IUserService userService;

        public CreateUserCommandHandler(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<UserReadModel> Handle(CreateUserCommand request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var userAggregate = await this.userService.CreateUser(
                request.UserData,
                request.AuthenticationMethodId,
                request.ExternalUserId);
            return userAggregate.LatestProjectedReadModel;
        }
    }
}
