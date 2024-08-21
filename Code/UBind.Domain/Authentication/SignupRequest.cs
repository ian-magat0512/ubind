// <copyright file="SignupRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using Newtonsoft.Json;

    /// <summary>
    /// Details for a user's sign up request.
    /// </summary>
    public class SignupRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignupRequest"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the signup request is for.</param>
        /// <param name="environment">The environment the signup request is for.</param>
        /// <param name="email">The user's requested email.</param>
        /// <param name="saltedHashedPassword">The user's salted hashed password.</param>
        /// <param name="fullName">The user's full name.</param>
        /// <param name="preferredName">The user's preferred name.</param>
        public SignupRequest(
            string tenantId,
            DeploymentEnvironment environment,
            string email,
            string saltedHashedPassword,
            string fullName,
            string preferredName = null)
        {
            this.TenantId = tenantId;
            this.Environment = environment;
            this.Email = email;
            this.SaltedHashedPassword = saltedHashedPassword;
            this.FullName = fullName;
            this.PreferredName = preferredName;
        }

        [JsonConstructor]
        private SignupRequest()
        {
        }

        /// <summary>
        /// Gets the ID of the tenant the signup request is for.
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the environment the signup request is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the user email.
        /// </summary>
        [JsonProperty]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the user's salted, hashed password.
        /// </summary>
        [JsonProperty]
        public string SaltedHashedPassword { get; private set; }

        /// <summary>
        /// Gets the user's full name.
        /// </summary>
        [JsonProperty]
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the user's preferred name.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; private set; }
    }
}
