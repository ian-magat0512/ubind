// <copyright file="AuthenticationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Resource model for login requests.
    /// </summary>
    public class AuthenticationModel
    {
        /// <summary>
        /// Gets or sets the email address the login attempt is for.
        /// </summary>
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the plaintext password the login attempt is using.
        /// </summary>
        [Required]
        public string PlaintextPassword { get; set; }

        /// <summary>
        /// Gets or sets the string tenant Id or Alias which the user is logging in to.
        /// </summary>
        // [Required]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the string tenant Id or Alias which the user is logging in to.
        /// Note: this is for backward compatibility only.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the organisation Id or Alias which the user is logging in to.
        /// </summary>
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets the organisation Id or Alias which the user is logging in to.
        /// Note: this is for backward compatibility only.
        /// </summary>
        public string OrganisationId { get; set; }
    }
}
