// <copyright file="UserSessionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Redis
{
    using System.Text.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Represents a user's session data.
    /// </summary>
    public class UserSessionModel
    {
        /// <summary>
        /// Creates a new UserSessionModel from a user and a list of effective permissions.
        /// </summary>
        /// <param name="user">The user read model.</param>
        /// <param name="permissions">The effective permissions of the user </param>
        /// <param name="timestamp">The current time.</param>
        public UserSessionModel(UserReadModel user, IEnumerable<Permission> permissions, Instant timestamp)
            : this(
                user.TenantId,
                Guid.NewGuid(),
                user.OrganisationId,
                user.Id,
                user.UserType,
                user.CustomerId,
                user.LoginEmail,
                user.DisplayName,
                permissions,
                timestamp,
                user.PasswordLastChangedTimestamp)
        {
        }

        public UserSessionModel(
            Guid tenantId,
            Guid id,
            Guid organisationId,
            Guid userId,
            string userType,
            Guid? customerId,
            string accountEmailAddress,
            string displayName,
            IEnumerable<Permission> permissions,
            Instant timestamp,
            Instant passwordLastChangedTimestamp)
        {
            this.TenantId = tenantId;
            this.Id = id;
            this.OrganisationId = organisationId;
            this.UserId = userId;
            this.UserType = userType;
            this.CustomerId = customerId;
            this.AccountEmailAddress = accountEmailAddress;
            this.Permissions = permissions.ToList();
            this.CreatedTimestamp = timestamp;
            this.LastUsedTimestamp = timestamp;
            this.PasswordLastChangedTimestamp = passwordLastChangedTimestamp;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Parameterless constructor for System.Text.Json.
        /// </summary>
        [JsonConstructor]
        public UserSessionModel()
        {
        }

        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid UserId { get; set; }

        public string UserType { get; set; }

        public Guid? CustomerId { get; set; }

        public string AccountEmailAddress { get; set; }

        public string DisplayName { get; set; }

        public List<Permission> Permissions { get; set; }

        public Guid? AuthenticationMethodId { get; set; }

        public AuthenticationMethodType? AuthenticationMethodType { get; set; }

        /// <summary>
        /// Gets or sets the created time as the number of ticks since the epoch for persistance.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the time the user session was created.
        /// </summary>
        [JsonIgnore]
        public Instant CreatedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch); }
            set { this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// Gets or sets the last time the session was used, e.g. by the user triggering an API call.
        /// </summary>
        /// <remarks> Primitive typed property for EF to store Last time Used.</remarks>
        public long LastUsedTicksSinceEpoch { get; set; }

        [JsonIgnore]
        public Instant LastUsedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.LastUsedTicksSinceEpoch); }
            set { this.LastUsedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        public long? PasswordLastChangedTicksSinceEpoch { get; set; }

        [JsonIgnore]
        public Instant? PasswordLastChangedTimestamp
        {
            get
            {
                if (this.PasswordLastChangedTicksSinceEpoch.HasValue)
                {
                    return Instant.FromUnixTimeTicks(this.PasswordLastChangedTicksSinceEpoch.Value);
                }

                return null;
            }

            set
            {
                this.PasswordLastChangedTicksSinceEpoch = value?.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets or sets the additional data to identify the SAML session if the user is logged in via SAML.
        /// This is needed so that if we receive a Single Logout Request from the IdP, we can match
        /// against the identifying information in this object to know which uBind Session to terminate.
        /// </summary>
        public SamlSessionData? SamlSessionData { get; set; }
    }
}
