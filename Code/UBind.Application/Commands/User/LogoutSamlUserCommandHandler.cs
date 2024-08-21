// <copyright file="LogoutSamlUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using UBind.Application.Models.Sso;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories.Redis;

    public class LogoutSamlUserCommandHandler : ICommandHandler<LogoutSamlUserCommand, string>
    {
        private readonly IUserSessionRepository userSessionRepository;
        private readonly ICqrsMediator cqrsMediator;

        public LogoutSamlUserCommandHandler(
            IUserSessionRepository userSessionRepository,
            ICqrsMediator cqrsMediator)
        {
            this.userSessionRepository = userSessionRepository;
            this.cqrsMediator = cqrsMediator;
        }

        public async Task<string> Handle(LogoutSamlUserCommand command, CancellationToken cancellationToken)
        {
            if (command.SamlSessions != null)
            {
                foreach (var session in command.SamlSessions)
                {
                    await this.userSessionRepository.DeleteSamlSession(command.TenantId, session);
                }
            }

            return await this.GenerateReturnUrl(command);
        }

        private async Task<string> GenerateReturnUrl(LogoutSamlUserCommand command)
        {
            string relayStateStr = command.SloResult.RelayState;
            RelayState? relayState = string.IsNullOrEmpty(relayStateStr)
                ? relayStateStr.FromJson<RelayState>()
                : null;
            Guid? portalId = relayState?.PortalId;
            string path = relayState?.Path ?? "login";
            return await this.cqrsMediator.Send(new GetPortalUrlQuery(
                command.TenantId,
                command.OrganisationId,
                portalId,
                relayState?.Environment ?? DeploymentEnvironment.Production,
                path));
        }
    }
}
