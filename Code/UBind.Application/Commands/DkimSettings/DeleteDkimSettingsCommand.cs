// <copyright file="DeleteDkimSettingsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for deleting DKIM settings.
    /// </summary>
    public class DeleteDkimSettingsCommand : ICommand<Unit>
    {
        public DeleteDkimSettingsCommand(
            Guid tenantId,
            Guid dkimSettingsId,
            Guid oganisationId)
        {
            this.TenantId = tenantId;
            this.DkimSettingsId = dkimSettingsId;
            this.OrganisationId = oganisationId;
        }

        /// <summary>
        /// Gets the DKIM settings Id.
        /// </summary>
        public Guid DkimSettingsId { get; private set; }

        /// <summary>
        /// Gets the Tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; private set; }
    }
}
