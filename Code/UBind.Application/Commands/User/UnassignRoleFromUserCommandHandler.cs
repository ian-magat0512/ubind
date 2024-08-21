// <copyright file="UnassignRoleFromUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.User;
    using UBind.Domain.Patterns.Cqrs;

    public class UnassignRoleFromUserCommandHandler : ICommandHandler<UnassignRoleFromUserCommand, Unit>
    {
        private readonly IUserService userService;

        public UnassignRoleFromUserCommandHandler(IUserService userService)
        {
            this.userService = userService;
        }

        public Task<Unit> Handle(UnassignRoleFromUserCommand command, CancellationToken cancellationToken)
        {
            this.userService.AddRoleToUser(command.TenantId, command.UserId, command.RoleId);
            return Task.FromResult(Unit.Value);
        }
    }
}
