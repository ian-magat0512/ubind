// <copyright file="SetDefaultOrganisationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Sets the default organisation of a tenant.
    /// </summary>
    [CreateTransactionThatSavesChangesIfNoneExists]
    public class SetDefaultOrganisationCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The organisation ID.</param>
        public SetDefaultOrganisationCommand(Guid tenantId, Guid organisationId)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the Tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the Organisation ID.
        /// </summary>
        public Guid OrganisationId { get; }
    }
}
