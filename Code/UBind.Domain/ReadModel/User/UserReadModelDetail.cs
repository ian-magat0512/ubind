// <copyright file="UserReadModelDetail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.User
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Detail for a user.
    /// </summary>
    public class UserReadModelDetail : IUserReadModelSummary
    {
        /// <summary>
        /// Gets or sets the organisation name.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets the organisation alias.
        /// </summary>
        public string OrganisationAlias { get; set; }

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
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <inheritdoc/>
        public DeploymentEnvironment? Environment { get; set; }

        /// <inheritdoc/>
        public Guid PersonId { get; set; }

        /// <inheritdoc/>
        public string LoginEmail { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerId { get; set; }

        /// <inheritdoc/>
        public string UserType { get; set; }

        [NotMapped]
        public PortalUserType PortalUserType => this.UserType.ToPortalUserType();

        /// <inheritdoc/>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the user would log into by default,
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <inheritdoc/>
        public bool HasBeenInvitedToActivate { get; set; }

        /// <inheritdoc/>
        public bool HasBeenActivated { get; set; }

        /// <inheritdoc/>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the time the user was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <inheritdoc/>
        public Guid? ProfilePictureId { get; set; }

        /// <summary>
        /// Gets the user status.
        /// </summary>
        public UserStatus UserStatus =>
            this.IsDisabled ? UserStatus.Deactivated :
            this.HasBeenActivated ? UserStatus.Active :
            this.HasBeenInvitedToActivate ? UserStatus.Invited :
            UserStatus.New;

        /// <summary>
        /// Gets or sets List of roles.
        /// </summary>
        public virtual ICollection<Role> Roles { get; set; } = new Collection<Role>();

        /// <summary>
        /// Gets or sets the list of linked identities, which represent the user's account in an external
        /// identity provider.
        /// </summary>
        public virtual ICollection<UserLinkedIdentityReadModel> LinkedIdentities { get; set; }
            = new Collection<UserLinkedIdentityReadModel>();

        /// <inheritdoc/>
        public long LastModifiedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the time the user was modified in ticks in the epoch.
        /// </summary>
        public Instant LastModifiedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.LastModifiedTicksSinceEpoch);
            }

            set
            {
                this.LastModifiedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Permission> GetPermissions()
        {
            return this.Roles.SelectMany(r => r.Permissions);
        }

        /// <inheritdoc/>
        public bool HasPermission(Permission permission)
        {
            return this.GetPermissions()
                .Contains(permission);
        }
    }
}
