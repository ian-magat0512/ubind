// <copyright file="CaptureSentryExceptionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sentry
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Services.Email;
    using UBind.Domain.Patterns.Cqrs;

    public class CaptureSentryExceptionCommandHandler : ICommandHandler<CaptureSentryExceptionCommand, Unit>
    {
        private readonly IErrorNotificationService errorNotificationService;

        public CaptureSentryExceptionCommandHandler(IErrorNotificationService errorNotificationService)
        {
            this.errorNotificationService = errorNotificationService;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(CaptureSentryExceptionCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.errorNotificationService.CaptureSentryException(command.Exception, command.Environment, command.AdditionalContext);
            return Task.FromResult(Unit.Value);
        }
    }
}
