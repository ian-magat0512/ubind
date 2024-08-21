// <copyright file="UserReadModel.cs" company="uBind">
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
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Read model for users.
    /// </summary>
    public class UserReadModel : PersonCommonProperties, IUserReadModelSummary, IDeletable
    {
        /// <summary>
        /// Initializes static properties.
        /// </summary>
        static UserReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReadModel"/> class.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="personData">The information of the person the user refers to.</param>
        /// <param name="customerId">The ID of the customer the user is for, if any, otherwise default.</param>
        /// <param name="portalId">The ID of the portal which the user should log in to.</param>
        /// <param name="createdTimestamp">The time the user was created.</param>
        /// <param name="environment">The data environment the user is associated with, or null if they can access all environments.</param>
        public UserReadModel(
            Guid id,
            PersonData personData,
            Guid? customerId,
            Guid? portalId,
            Instant createdTimestamp,
            UserType userType,
            DeploymentEnvironment? environment = null)
        {
            this.Id = id;
            this.TenantId = personData.TenantId;
            this.OrganisationId = personData.OrganisationId;
            this.PortalId = portalId;
            this.Environment = environment;
            this.PersonId = personData.PersonId;
            this.CustomerId = customerId;
            this.FullName = personData.FullName;
            this.PreferredName = personData.PreferredName;
            this.Email = personData.Email;
            this.LoginEmail = personData.LoginEmail ?? personData.Email;
            this.AlternativeEmail = personData.AlternativeEmail;
            this.MobilePhoneNumber = personData.MobilePhone;
            this.HomePhoneNumber = personData.HomePhone;
            this.WorkPhoneNumber = personData.WorkPhone;
            this.CreatedTimestamp = createdTimestamp;
            this.NamePrefix = personData.NamePrefix;
            this.FirstName = personData.FirstName;
            this.MiddleNames = personData.MiddleNames;
            this.LastName = personData.LastName;
            this.NameSuffix = personData.NameSuffix;
            this.Company = personData.Company;
            this.Title = personData.Title;
            this.UserType = userType.Humanize();
        }

        /// <summary>
        /// Parameterless constructor to allow EF use lazy loading.
        /// </summary>
        public UserReadModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReadModel"/> class.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        protected UserReadModel(Guid userId)
        {
            this.Id = userId;
        }

        /// <summary>
        /// Gets the environment the user belongs to, or null if the user has access to all environments.
        /// </summary>
        public DeploymentEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the ID of the person the user refers to.
        /// </summary>
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets or sets the email the user logs in with.
        /// </summary>
        public string LoginEmail { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer the user relates to, if any, otherwise default.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the type of the user.
        /// </summary>
        public string UserType { get; set; }

        [NotMapped]
        public PortalUserType PortalUserType => this.UserType.ToPortalUserType();

        /// <summary>
        /// Gets or sets a value indicating whether the user has been disabled or deactivated.
        /// Note: this is different to the user being locked out temporarily due to too many failed login attempts..
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the user would log into by default,
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been invited to activate.
        /// </summary>
        public bool HasBeenInvitedToActivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been activated.
        /// </summary>
        public bool HasBeenActivated { get; set; }

        /// <summary>
        /// Gets or sets the user's profile picture id.
        /// </summary>
        public System.Guid? ProfilePictureId { get; set; }

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

        /// <summary>
        /// Gets the password last changed time the user was modified in ticks since epoch.
        /// </summary>
        public long PasswordLastChangedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the password last changed time the user was modified in ticks in the epoch.
        /// </summary>
        public Instant PasswordLastChangedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.PasswordLastChangedTicksSinceEpoch);
            }

            set
            {
                this.PasswordLastChangedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Retrieves the permissions of the user.
        /// </summary>
        /// <returns>List of permissions of the user.</returns>
        public IEnumerable<Permission> GetPermissions()
        {
            return this.Roles.SelectMany(r => r.Permissions);
        }

        /// <summary>
        /// Gets a value indicating whether the user has a given permission.
        /// </summary>
        /// <param name="permission">The permission.</param>
        /// <returns>True if the user has that permission.</returns>
        public bool HasPermission(Permission permission)
        {
            return this.GetPermissions().Contains(permission);
        }

        public void UpdateEnvironment(DeploymentEnvironment environment)
        {
            this.Environment = environment;
        }
    }
}
