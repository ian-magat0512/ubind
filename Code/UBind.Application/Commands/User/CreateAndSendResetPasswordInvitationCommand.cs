// <copyright file="CreateAndSendResetPasswordInvitationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    [RetryOnDbException(5)]
    public class CreateAndSendResetPasswordInvitationCommand : ICommand
    {
        public CreateAndSendResetPasswordInvitationCommand(
            Guid tenantId,
            Guid organisationId,
            string email,
            DeploymentEnvironment environment,
            Guid? productId = null,
            bool isPasswordExpired = false)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Email = email;
            this.Environment = environment;
            this.ProductId = productId;
            this.IsPasswordExpired = isPasswordExpired;
        }

        public Guid TenantId { get; set; }

        public Guid OrganisationId { get; set; }

        public string Email { get; set; }

        public Guid? ProductId { get; set; }

        public bool IsPasswordExpired { get; set; }

        public DeploymentEnvironment Environment { get; set; }
    }
}
