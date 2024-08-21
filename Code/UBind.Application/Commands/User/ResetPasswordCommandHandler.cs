// <copyright file="ResetPasswordCommandHandler.cs" company="uBind">
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

    /// <summary>
    /// Command to reset a users password to a new one, using a reset password invitation token.
    /// </summary>
    public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserPasswordResetInvitationService userPasswordResetInvitationService;

        public ResetPasswordCommandHandler(
            IUserPasswordResetInvitationService userPasswordResetInvitationService)
        {
            this.userPasswordResetInvitationService = userPasswordResetInvitationService;
        }

        public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.userPasswordResetInvitationService.SetPasswordFromPasswordReset(
                command.TenantId, command.UserId, command.InvitationId, command.ClearTextPassword);

            return Unit.Value;
        }
    }
}
