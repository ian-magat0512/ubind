// <copyright file="UserPasswordResetEmailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// User password reset model.
    /// </summary>
    public class UserPasswordResetEmailModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPasswordResetEmailModel"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="preferredName">The preferred name of the user.</param>
        /// <param name="fullName">The full name of the user.</param>
        /// <param name="invitationId">The invitation ID of the user.</param>
        /// <param name="tenantAlias">The alias of the user's tenant.</param>
        /// <param name="environment">The environemnt of the user.</param>
        public UserPasswordResetEmailModel(
            Guid userId,
            string email,
            string preferredName,
            string fullName,
            string invitationId,
            string tenantAlias,
            DeploymentEnvironment environment)
        {
            this.UserId = userId;
            this.Email = email;
            this.PreferredName = preferredName;
            this.FullName = fullName;
            this.InvitationId = invitationId;
            this.TenantAlias = tenantAlias;
            this.Environment = environment;
        }

        /// <summary>
        /// Gets the ID of the user.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the email of the user.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the preferred name of the user.
        /// </summary>
        public string PreferredName { get; private set; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the invitation ID.
        /// </summary>
        public string InvitationId { get; private set; }

        /// <summary>
        /// Gets the alias for the tenant of the user.
        /// </summary>
        public string TenantAlias { get; private set; }

        /// <summary>
        /// Gets the environment of the user.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }
    }
}
