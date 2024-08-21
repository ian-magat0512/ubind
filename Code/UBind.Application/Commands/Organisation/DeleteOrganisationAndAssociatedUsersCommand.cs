// <copyright file="DeleteOrganisationAndAssociatedUsersCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command for marking users as deleted by organisation.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class DeleteOrganisationAndAssociatedUsersCommand : ICommand<IOrganisationReadModelSummary>
    {
        public DeleteOrganisationAndAssociatedUsersCommand(Guid tenantId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
        }

        public Guid TenantId { get; private set; }

        public Guid OrganisationId { get; private set; }
    }
}
