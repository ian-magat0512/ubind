// <copyright file="TransferUserToOtherOrganisationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    [CreateTransactionThatSavesChangesIfNoneExists]
    public class TransferUserToOtherOrganisationCommand : ICommand
    {
        public TransferUserToOtherOrganisationCommand(Guid tenantId, Guid userId, Guid toOrganisationId, bool includeCustomers)
        {
            this.TenantId = tenantId;
            this.UserId = userId;
            this.ToOrganisationId = toOrganisationId;
            this.IncludeCustomers = includeCustomers;
        }

        public Guid TenantId { get; }

        public Guid UserId { get; }

        public Guid ToOrganisationId { get; }

        public bool IncludeCustomers { get; }
    }
}
