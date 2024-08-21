// <copyright file="UpdateUserCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserReadModel>
    {
        private readonly IUserService userService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        public UpdateUserCommandHandler(
            IUserService userService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.userService = userService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        public async Task<UserReadModel> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var userAggregate = await this.userService.Update(command.TenantId, command.UserId, command.UserUpdateModel, command.Properties);
            return userAggregate.LatestProjectedReadModel;
        }
    }
}
