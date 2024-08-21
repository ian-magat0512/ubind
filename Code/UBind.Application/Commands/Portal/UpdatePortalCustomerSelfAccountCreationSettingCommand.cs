// <copyright file="UpdatePortalCustomerSelfAccountCreationSettingCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command model for updating the account creation from login page setting.
    /// </summary>
    public class UpdatePortalCustomerSelfAccountCreationSettingCommand : ICommand<Unit>
    {
        public UpdatePortalCustomerSelfAccountCreationSettingCommand(
            Guid tenantId, Guid organisationId, bool allowCustomerSelfAccountCreation)
        {
            this.TenantId = tenantId;
            this.PortalId = organisationId;
            this.AllowCustomerSelfAccountCreation = allowCustomerSelfAccountCreation;
        }

        public Guid TenantId { get; private set; }

        public Guid PortalId { get; private set; }

        public bool AllowCustomerSelfAccountCreation { get; private set; }
    }
}
