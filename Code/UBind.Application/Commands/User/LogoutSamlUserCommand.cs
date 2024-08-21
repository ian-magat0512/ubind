// <copyright file="LogoutSamlUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using ComponentSpace.Saml2;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Redis;

    public class LogoutSamlUserCommand : ICommand<string> // returns the URL to redirect to
    {
        public LogoutSamlUserCommand(Guid tenantId, Guid organisationId, ISloResult sloResult, IEnumerable<SamlSessionData> samlSessions)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.SloResult = sloResult;
            this.SamlSessions = samlSessions;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public ISloResult SloResult { get; }

        public IEnumerable<SamlSessionData> SamlSessions { get; }
    }
}
