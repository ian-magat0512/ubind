// <copyright file="UserDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;
    using NodaTime;

    /// <summary>
    /// A drop model for user.
    /// </summary>
    public class UserDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDrop"/> class.
        /// </summary>
        /// <param name="id">The user's Id.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="alternativeEmail">The user's alternative email.</param>
        /// <param name="preferredName">The user's preferred name.</param>
        /// <param name="fullName">The user's full name.</param>
        /// <param name="namePrefix">The user's name prefix.</param>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="middleNames">The user's middle names.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <param name="nameSuffix">The user's name suffix.</param>
        /// <param name="company">the user's company.</param>
        /// <param name="greetingName">The greeting name.</param>
        /// <param name="title">The user's title.</param>
        /// <param name="mobilePhone">The user mobile num.</param>
        /// <param name="workPhone">The user work phone.</param>
        /// <param name="homePhone">The user home phone.</param>
        /// <param name="blocked">The indicator if user is blocked.</param>
        /// <param name="createdTimestamp">The user created time.</param>
        /// <param name="tenantId">The user tenant id.</param>
        /// <param name="environment">The user environment.</param>
        public UserDrop(
            Guid tenantId,
            string environment,
            Guid id,
            string email,
            string alternativeEmail,
            string preferredName,
            string fullName,
            string namePrefix,
            string firstName,
            string middleNames,
            string lastName,
            string nameSuffix,
            string greetingName,
            string company,
            string title,
            string mobilePhone,
            string workPhone,
            string homePhone,
            bool blocked,
            Instant createdTimestamp)
        {
            this.Id = id;
            this.Email = email;
            this.AlternativeEmail = alternativeEmail;
            this.PreferredName = preferredName ?? greetingName;
            this.FullName = fullName;
            this.NamePrefix = namePrefix;
            this.FirstName = firstName;
            this.MiddleNames = middleNames;
            this.LastName = lastName;
            this.NameSuffix = nameSuffix;
            this.GreetingName = greetingName;
            this.Company = company;
            this.Title = title;
            this.MobilePhone = mobilePhone;
            this.WorkPhone = workPhone;
            this.HomePhone = homePhone;
            this.Blocked = blocked;
            this.CreatedTimestamp = createdTimestamp;
            this.TenantId = tenantId;
            this.Environment = environment;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets gets the email of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the alternative email of the user.
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the preferred name of the user.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the user's name prefix.
        /// </summary>
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's middle names.
        /// </summary>
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's name suffix.
        /// </summary>
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the user's greeting name.
        /// </summary>
        public string GreetingName { get; set; }

        /// <summary>
        /// Gets or sets the user's company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the user's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets gets the Id reference of the user picture.
        /// </summary>
        public Guid PictureId { get; set; }

        /// <summary>
        /// Gets or sets the role type of the user.
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone number of the user.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets gets the home phone number of the user.
        /// </summary>
        public string HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the work phone number of the user.
        /// </summary>
        public string WorkPhone { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the deployment environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is blocked or not.
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets the created date time of the user.
        /// </summary>
        public Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets the created date time of the user.
        /// </summary>
        [Obsolete("Please use CreatedTimestamp instead.")]
        public Instant CreationTime => this.CreatedTimestamp;
    }
}
