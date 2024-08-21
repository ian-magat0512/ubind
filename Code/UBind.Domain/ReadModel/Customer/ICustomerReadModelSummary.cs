// <copyright file="ICustomerReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Customer
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    public interface ICustomerReadModelSummary
    {
        Instant CreatedTimestamp { get; }

        long CreatedTicksSinceEpoch { get; set; }

        DeploymentEnvironment Environment { get; set; }

        Guid Id { get; }

        bool IsDeleted { get; set; }

        bool IsTestData { get; set; }

        long LastModifiedTicksSinceEpoch { get; set; }

        Instant LastModifiedTimestamp { get; }

        string OwnerFullName { get; set; }

        Guid OwnerPersonId { get; set; }

        Guid? OwnerUserId { get; set; }

        Guid PrimaryPersonId { get; set; }

        bool UserHasBeenActivated { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the customer would log into by default,
        /// If there is no specific portal required for a given product.
        /// This would be null if the customer doesn't log into a portal, or the customer
        /// is expected to login to the default portal for the tenanacy.
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        Guid? PortalId { get; set; }

        bool UserHasBeenInvitedToActivate { get; set; }

        Guid? UserId { get; set; }

        bool UserIsBlocked { get; set; }

        string FullName { get; set; }

        string NamePrefix { get; set; }

        string FirstName { get; set; }

        string MiddleNames { get; set; }

        string LastName { get; set; }

        string NameSuffix { get; set; }

        string Company { get; set; }

        string Title { get; set; }

        string PreferredName { get; set; }

        string Email { get; set; }

        string AlternativeEmail { get; set; }

        string MobilePhoneNumber { get; set; }

        string HomePhoneNumber { get; set; }

        string WorkPhoneNumber { get; set; }

        Guid TenantId { get; set; }

        string DisplayName { get; set; }

        Guid OrganisationId { get; set; }

        ICollection<PersonReadModel> People { get; set; }
    }
}
