// <copyright file="SendDkimTestEmailCommand.cs" company="uBind">
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
    /// Command for sending DKIM test email.
    /// </summary>
    public class SendDkimTestEmailCommand : ICommand<Unit>
    {
        public SendDkimTestEmailCommand(
            Guid tenantId,
            Guid dkimSettingsId,
            Guid organisationId,
            string recipientEmailAddress,
            string senderEmailAddress)
        {
            this.DkimSettingsId = dkimSettingsId;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.RecipientEmailAddress = recipientEmailAddress;
            this.SenderEmailAddress = senderEmailAddress;
        }

        /// <summary>
        /// Gets the DKIM settings Id.
        /// </summary>
        public Guid DkimSettingsId { get; private set; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the recipient email address.
        /// </summary>
        public string RecipientEmailAddress { get; private set; }

        /// <summary>
        /// Gets the Sender email address.
        /// </summary>
        public string SenderEmailAddress { get; private set; }
    }
}
