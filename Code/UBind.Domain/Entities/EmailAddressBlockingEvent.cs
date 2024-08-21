// <copyright file="EmailAddressBlockingEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;
    using UBind.Domain.Authentication;

    /// <summary>
    /// Represents event that occured on a login attempt.
    /// </summary>
    public class EmailAddressBlockingEvent : Entity<Guid>
    {
        private EmailAddressBlockingEvent(
            Guid tenantId,
            Guid organisationId,
            string email,
            bool isEmailAddressBlocked,
            int emailAddressUnblockedReason,
            Instant timestamp)
            : base(Guid.NewGuid(), timestamp)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.EmailAddress = email;
            this.IsEmailAddressBlocked = isEmailAddressBlocked;
            this.EmailAddressUnblockedReason = emailAddressUnblockedReason;
        }

        // Parameterless constructor for EF.
        private EmailAddressBlockingEvent()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the ID of the tenant the login attempt was for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets or sets the alias of the organisation the login attempt was for.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets the email used in the login attempt.
        /// </summary>
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the account is locked.
        /// </summary>
        public bool IsEmailAddressBlocked { get; private set; }

        /// <summary>
        /// Gets a value indicating the reason for locked/unlocked status change.
        /// </summary>
        public int EmailAddressUnblockedReason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressBlockingEvent"/> class for account locking.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant with the email address being blocked.</param>
        /// <param name="organisationId">The Id of the organisation that account was locked for.</param>
        /// <param name="email">The email address.</param>
        /// <param name="blockReason">The EmailAddressUnblockedReason code for the email address block event.</param>
        /// <param name="timestamp">The time of the event.</param>
        /// <returns>A new instance of the <see cref="EmailAddressBlockingEvent"/> class for email address blocking.</returns>
        public static EmailAddressBlockingEvent EmailAddressBlocked(
            Guid tenantId,
            Guid organisationId,
            string email,
            EmailAddressUnblockedReason blockReason,
            Instant timestamp)
        {
            return new EmailAddressBlockingEvent(tenantId, organisationId, email, true, (int)blockReason, timestamp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressBlockingEvent"/> class for account unlocking.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant with the email address being blocked.</param>
        /// <param name="organisationId">The Id of the organisation that account was locked for.</param>
        /// <param name="email">The email address.</param>
        /// <param name="unblockReason">The EmailAddressUnblockedReason code for the email address unblock event.</param>
        /// <param name="timestamp">The time of the event.</param>
        /// <returns>A new instance of the <see cref="EmailAddressBlockingEvent"/> class for email address blocking.</returns>
        public static EmailAddressBlockingEvent EmailAddressUnblocked(
            Guid tenantId,
            Guid organisationId,
            string email,
            EmailAddressUnblockedReason unblockReason,
            Instant timestamp)
        {
            return new EmailAddressBlockingEvent(tenantId, organisationId, email, false, (int)unblockReason, timestamp);
        }
    }
}
