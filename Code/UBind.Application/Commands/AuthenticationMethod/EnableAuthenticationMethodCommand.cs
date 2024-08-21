// <copyright file="EnableAuthenticationMethodCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.AuthenticationMethod
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class EnableAuthenticationMethodCommand : ICommand<AuthenticationMethodReadModelSummary>
    {
        public EnableAuthenticationMethodCommand(Guid tenantId, Guid organisationId, Guid authenticationMethodId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.AuthenticationMethodId = authenticationMethodId;
        }

        public Guid TenantId { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid AuthenticationMethodId { get; set; }
    }
}
