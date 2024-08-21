// <copyright file="UpdateOrganisationRenewalInvitationEmailsSettingCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Organisation
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command model for updating the renewal invitation emails setting.
    /// </summary>
    public class UpdateOrganisationRenewalInvitationEmailsSettingCommand : ICommand<Unit>
    {
        public UpdateOrganisationRenewalInvitationEmailsSettingCommand(
            Guid tenantId, Guid organisationId, bool allowOrganisationRenewalInvitation)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.AllowOrganisationRenewalInvitation = allowOrganisationRenewalInvitation;
        }

        public Guid TenantId { get; private set; }

        public Guid OrganisationId { get; private set; }

        public bool AllowOrganisationRenewalInvitation { get; private set; }
    }
}
