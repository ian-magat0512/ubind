// <copyright file="UserModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Common Model for Users.
    /// </summary>
    public class UserModel : PersonCommonProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserModel"/> class.
        /// </summary>
        /// <param name="user">Auth0 management api user model.</param>
        /// <param name="person">The person the user refers to.</param>
        public UserModel(UserAggregate user, PersonAggregate person)
        {
            this.Id = user.Id;
            this.Email = person.Email;
            this.CreatedTimestamp = user.CreatedTimestamp;
            this.Blocked = user.Blocked;
            this.PreferredName = person.PreferredName;
            this.FullName = person.FullName;
            this.NamePrefix = person.NamePrefix;
            this.FirstName = person.FirstName;
            this.MiddleNames = person.MiddleNames;
            this.LastName = person.LastName;
            this.NameSuffix = person.NameSuffix;
            this.GreetingName = person.GreetingName;
            this.Company = person.Company;
            this.Title = person.Title;
            this.AlternativeEmail = person.AlternativeEmail;
            this.MobilePhoneNumber = person.MobilePhone;
            this.HomePhoneNumber = person.HomePhone;
            this.WorkPhoneNumber = person.WorkPhone;
            this.UserType = user.UserType;
            this.TenantId = user.TenantId;
            this.OrganisationId = user.OrganisationId;
            this.TenantId = user.TenantId;
            this.Environment = user.Environment;
            this.ProfilePictureId = user.ProfilePictureId;
            this.PersonId = user.PersonId;
            this.UserStatus = user.UserStatus.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserModel"/> class.
        /// </summary>
        /// <param name="user">The user read model.</param>
        public UserModel(UserReadModel user)
        {
            this.Id = user.Id;
            this.Email = user.Email;
            this.FullName = user.FullName;
            this.Blocked = user.IsDisabled;
            this.TenantId = user.TenantId;
            this.OrganisationId = user.OrganisationId;
            this.UserStatus = user.UserStatus.ToString();
            this.LastModifiedTicksSinceEpoch = user.LastModifiedTicksSinceEpoch;
        }

        /// <summary>
        /// Gets the person id.
        /// </summary>
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets or sets the user's greeting name.
        /// </summary>
        public string GreetingName { get; set; }

        /// <summary>
        /// Gets the deployment environment the user belogs to, or null if the user has access to all environments.
        /// </summary>
        public DeploymentEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the Id reference of the user picture.
        /// </summary>
        public Guid PictureId { get; private set; }

        /// <summary>
        /// Gets the role type of the user.
        /// </summary>
        public string UserType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is blocked or not.
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets the profile picture id of the user.
        /// </summary>
        public Guid ProfilePictureId { get; private set; }

        /// <summary>
        /// Gets or sets the user's status.
        /// </summary>
        public string UserStatus { get; set; }
    }
}
