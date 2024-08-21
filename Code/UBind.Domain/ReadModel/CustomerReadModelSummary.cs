// <copyright file="CustomerReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadModel.Customer;

    /// <inheritdoc/>
    public class CustomerReadModelSummary : ICustomerReadModelSummary
    {
        /// <inheritdoc/>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }
        }

        /// <inheritdoc/>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <inheritdoc/>
        public DeploymentEnvironment Environment { get; set; }

        /// <inheritdoc/>
        public Guid Id => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsDeleted { get; set; }

        /// <inheritdoc/>
        public bool IsTestData { get; set; }

        /// <inheritdoc/>
        public long LastModifiedTicksSinceEpoch { get; set; }

        /// <inheritdoc/>
        public Instant LastModifiedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.LastModifiedTicksSinceEpoch);
            }
        }

        /// <inheritdoc/>
        public string OwnerFullName { get; set; }

        /// <inheritdoc/>
        public Guid OwnerPersonId { get; set; }

        /// <inheritdoc/>
        public Guid? OwnerUserId { get; set; }

        /// <inheritdoc/>
        public Guid PrimaryPersonId { get; set; }

        /// <inheritdoc/>
        public bool UserHasBeenActivated { get; set; }

        /// <inheritdoc/>
        public Guid? PortalId { get; set; }

        /// <inheritdoc/>
        public bool UserHasBeenInvitedToActivate { get; set; }

        /// <inheritdoc/>
        public Guid? UserId { get; set; }

        /// <inheritdoc/>
        public bool UserIsBlocked { get; set; }

        /// <inheritdoc/>
        public string FullName { get; set; }

        /// <inheritdoc/>
        public string NamePrefix { get; set; }

        /// <inheritdoc/>
        public string FirstName { get; set; }

        /// <inheritdoc/>
        public string MiddleNames { get; set; }

        /// <inheritdoc/>
        public string LastName { get; set; }

        /// <inheritdoc/>
        public string NameSuffix { get; set; }

        /// <inheritdoc/>
        public string Company { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string PreferredName { get; set; }

        /// <inheritdoc/>
        public string Email { get; set; }

        /// <inheritdoc/>
        public string AlternativeEmail { get; set; }

        /// <inheritdoc/>
        public string MobilePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string HomePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string WorkPhoneNumber { get; set; }

        /// <inheritdoc/>
        public Guid TenantId { get; set; }

        /// <inheritdoc/>
        public string DisplayName { get; set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public ICollection<PersonReadModel> People { get; set; }
    }
}
