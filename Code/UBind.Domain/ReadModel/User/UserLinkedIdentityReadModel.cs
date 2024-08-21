// <copyright file="UserLinkedIdentityReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.User
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a link between a uBind user account and an account within an external identity provider.
    /// </summary>
    public class UserLinkedIdentityReadModel
    {
        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public Guid AuthenticationMethodId { get; set; }

        public string AuthenticationMethodName { get; set; }

        public string AuthenticationMethodTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique ID for the user in the external identity provider's system.
        /// </summary>
        public string UniqueId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserReadModel User { get; set; }
    }
}
