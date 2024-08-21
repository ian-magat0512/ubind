// <copyright file="UsersByEmailRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Resource model for users.
    /// </summary>
    public class UsersByEmailRequestModel
    {
        /// <summary>
        /// Gets or sets the Email of user.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the user.
        /// Can be a Guid or the alias of a tenant.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user is blocked.
        /// </summary>
        public bool? Blocked { get; set; }
    }
}
