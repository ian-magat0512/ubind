// <copyright file="UserLoginEmail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using NodaTime;

    /// <summary>
    /// For enforcing uniqueness constraint on user login emails.
    /// </summary>
    public class UserLoginEmail : MutableEntity<Guid>, IEntityReadModel<Guid>, IDeletable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginEmail"/> class.
        /// </summary>
        /// <param name="userAggregate">The user aggregate.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        public UserLoginEmail(UserAggregate userAggregate, Instant createdTimestamp)
            : base(userAggregate.Id, createdTimestamp)
        {
            this.TenantId = userAggregate.TenantId;
            this.OrganisationId = userAggregate.OrganisationId;
            this.LoginEmail = userAggregate.LoginEmail;
            this.SaltedHashedPassword = userAggregate.CurrentSaltedHashedPassword;
        }

        public UserLoginEmail(Guid tenantId, Guid id, Instant createdTimestamp, Guid organisationId, string loginEmail)
            : base(id, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.LoginEmail = loginEmail;
        }

        // Parameterless constructor for EF.
        private UserLoginEmail()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets the Id of the tenant the user belongs to.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Id of the organisation the user belongs to.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets or sets the email the user logs in with.
        /// </summary>
        public string LoginEmail { get; set; }

        public string? SaltedHashedPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user login is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        public void SetOrganisationId(Guid organisationId)
        {
            this.OrganisationId = organisationId;
        }
    }
}
