// <copyright file="IUserReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.User
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Common properties for user read models.
    /// </summary>
    public interface IUserReadModelSummary
    {
        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Gets or sets the user's name prefix.
        /// </summary>
        string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's middle names.
        /// </summary>
        string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's name suffix.
        /// </summary>
        string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the user's company.
        /// </summary>
        string Company { get; set; }

        /// <summary>
        /// Gets or sets the user's title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the user's Email.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's alternate email.
        /// </summary>
        string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the user's mobile phone number.
        /// </summary>
        string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's home phone number.
        /// </summary>
        string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's work phone number.
        /// </summary>
        string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the Id of the tenant of the user.
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the user's deserialized repeating fields string.
        /// </summary>
        Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets the user's ID.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the environment the user belongs to, or null if the user has access to all environments.
        /// </summary>
        DeploymentEnvironment? Environment { get; }

        /// <summary>
        /// Gets the ID of the person the user refers to.
        /// </summary>
        Guid PersonId { get; }

        /// <summary>
        /// Gets or sets the email the user logs in with.
        /// </summary>
        string LoginEmail { get; set; }

        // <summary>
        // Gets or sets the ID of the customer the user relates to, if any, otherwise default.
        // </summary>
        Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the user's type.
        /// </summary>
        string UserType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been disabled or deactivated.
        /// Note: this is different to the user being locked out temporarily due to too many failed login attempts..
        /// </summary>
        bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the user would log into by default,
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been invited to activate.
        /// </summary>
        bool HasBeenInvitedToActivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been activated.
        /// </summary>
        bool HasBeenActivated { get; set; }

        /// <summary>
        /// Gets or sets the time the user was created in ticks in the epoch.
        /// </summary>
        /// <remarks>Exposed for efficient querying (in future).</remarks>
        long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the time the user was created.
        /// </summary>
        Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the user's profile picture id.
        /// </summary>
        System.Guid? ProfilePictureId { get; set; }

        /// <summary>
        /// Gets the user status.
        /// </summary>
        UserStatus UserStatus { get; }

        /// <summary>
        /// Gets or sets List of roles.
        /// </summary>
        ICollection<Role> Roles { get; set; }

        /// <summary>
        /// Gets the time the user was modified in ticks since epoch.
        /// </summary>
        long LastModifiedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets or sets the time the user was modified in ticks in the epoch.
        /// </summary>
        Instant LastModifiedTimestamp { get; set; }

        /// <summary>
        /// Retrieves the permissions of the user.
        /// </summary>
        /// <returns>List of permissions of the user.</returns>
        IEnumerable<Permission> GetPermissions();

        /// <summary>
        /// Gets a value indicating whether the user has a given permission.
        /// </summary>
        /// <param name="permission">The permission.</param>
        /// <returns>True if the user has that permission.</returns>
        bool HasPermission(Permission permission);
    }
}
