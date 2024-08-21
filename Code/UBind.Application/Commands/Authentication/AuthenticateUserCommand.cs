// <copyright file="AuthenticateUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Authentication
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Command for authenticating users.
    /// </summary>
    public class AuthenticateUserCommand : ICommand<UserReadModel>
    {
        public AuthenticateUserCommand(
            Guid tenantId,
            Guid organisationId,
            string email,
            string plaintextPassword)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.Email = email;
            this.PlaintextPassword = plaintextPassword;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public string Email { get; }

        public string PlaintextPassword { get; }
    }
}
