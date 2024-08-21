// <copyright file="LogoutCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Command to log a user out, by deleting their session token from the database.
    /// </summary>
    public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Unit>
    {
        private readonly IUserSessionService userSessionService;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;

        public LogoutCommandHandler(
            IUserSessionService userSessionService,
            IUserSystemEventEmitter userSystemEventEmitter)
        {
            this.userSessionService = userSessionService;
            this.userSystemEventEmitter = userSystemEventEmitter;
        }

        public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (command.User == null)
            {
                return Unit.Value;
            }

            var sessionId = command.User.SessionId();
            if (sessionId == null && sessionId == default(Guid))
            {
                return Unit.Value;
            }

            bool isToRecordUserSessionInvalidated = await this.IsSessionValid(command.User);
            await this.userSessionService.Delete(command.User);
            var userId = command.User.GetId();
            if (!userId.HasValue)
            {
                return Unit.Value;
            }

            List<SystemEventType> eventTypes = new List<SystemEventType>();
            eventTypes.Add(SystemEventType.UserLoggedOut);
            if (isToRecordUserSessionInvalidated)
            {
                eventTypes.Add(SystemEventType.UserSessionInvalidated);
            }

            await this.userSystemEventEmitter.CreateAndEmitSystemEvents(command.User.GetTenantId(), userId.Value, eventTypes);
            return Unit.Value;
        }

        private async Task<bool> IsSessionValid(ClaimsPrincipal user)
        {
            var userSessionModel = await this.userSessionService.Get(user);
            return userSessionModel != null;
        }
    }
}
