// <copyright file="PersonCustomerCommonPropertiesMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class PersonAggregate
    {
        public class PersonCustomerCommonPropertiesMigratedEvent
            : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonCustomerCommonPropertiesMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant.</param>
            /// <param name="personId">The Id of the person.</param>
            /// <param name="userId">The user Id of the person (nullable).</param>
            /// <param name="fullName">The full name of the person.</param>
            /// <param name="preferredName">The preferred name of the person.</param>
            /// <param name="namePrefix">The name prefix of the person.</param>
            /// <param name="nameSuffix">The name suffix of the person.</param>
            /// <param name="firstName">The first name of the person.</param>
            /// <param name="lastName">The last name of the person.</param>
            /// <param name="middleNames">The middle name of the person.</param>
            /// <param name="company">The company of the person.</param>
            /// <param name="title">The title of the person.</param>
            /// <param name="email">The email of the person.</param>
            /// <param name="alternativeEmail">The alternative email of the person.</param>
            /// <param name="mobilePhoneNumber">The mobile phone number of the person.</param>
            /// <param name="homePhoneNumber">The home phone number of the person.</param>
            /// <param name="workPhoneNumber">The work phone number of the person.</param>
            /// <param name="userHasBeenInvitedToActivate">Indicates whether the user has been invited to activated.</param>
            /// <param name="userHasBeenActivated">Indicates whether the user has been activated.</param>
            /// <param name="userIsBlocked">Indicates whether the user is blocked.</param>
            /// <param name="performingUserId">The Id of the performing user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PersonCustomerCommonPropertiesMigratedEvent(
                Guid tenantId,
                Guid personId,
                Guid? userId,
                string fullName,
                string preferredName,
                string namePrefix,
                string nameSuffix,
                string firstName,
                string lastName,
                string middleNames,
                string company,
                string title,
                string email,
                string alternativeEmail,
                string mobilePhoneNumber,
                string homePhoneNumber,
                string workPhoneNumber,
                bool userHasBeenInvitedToActivate,
                bool userHasBeenActivated,
                bool userIsBlocked,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.UserId = userId;
                this.FullName = fullName;
                this.PreferredName = preferredName;
                this.NamePrefix = namePrefix;
                this.NameSuffix = nameSuffix;
                this.FirstName = firstName;
                this.LastName = lastName;
                this.MiddleNames = middleNames;
                this.Company = company;
                this.Title = title;
                this.Email = email;
                this.AlternativeEmail = alternativeEmail;
                this.MobilePhoneNumber = mobilePhoneNumber;
                this.HomePhoneNumber = homePhoneNumber;
                this.WorkPhoneNumber = workPhoneNumber;
                this.UserHasBeenInvitedToActivate = userHasBeenInvitedToActivate;
                this.UserHasBeenActivated = userHasBeenActivated;
                this.UserIsBlocked = userIsBlocked;
            }

            /// <summary>
            /// Gets the Id of the user.
            /// </summary>
            [JsonProperty]
            public Guid? UserId { get; private set; }

            /// <summary>
            /// Gets the full name of the customer.
            /// </summary>
            [JsonProperty]
            public string FullName { get; private set; }

            /// <summary>
            /// Gets the preferred name of the customer.
            /// </summary>
            [JsonProperty]
            public string PreferredName { get; private set; }

            /// <summary>
            /// Gets the name prefix of the customer.
            /// </summary>
            [JsonProperty]
            public string NamePrefix { get; private set; }

            /// <summary>
            /// Gets the name suffix of the customer.
            /// </summary>
            [JsonProperty]
            public string NameSuffix { get; private set; }

            /// <summary>
            /// Gets the first name of the customer.
            /// </summary>
            [JsonProperty]
            public string FirstName { get; private set; }

            /// <summary>
            /// Gets the last name of the customer.
            /// </summary>
            [JsonProperty]
            public string LastName { get; private set; }

            /// <summary>
            /// Gets the middle name of the customer.
            /// </summary>
            [JsonProperty]
            public string MiddleNames { get; private set; }

            /// <summary>
            /// Gets the company of the customer.
            /// </summary>
            [JsonProperty]
            public string Company { get; private set; }

            /// <summary>
            /// Gets the title of the customer.
            /// </summary>
            [JsonProperty]
            public string Title { get; private set; }

            /// <summary>
            /// Gets the email of the customer.
            /// </summary>
            [JsonProperty]
            public string Email { get; private set; }

            /// <summary>
            /// Gets the alternative email of the customer.
            /// </summary>
            [JsonProperty]
            public string AlternativeEmail { get; private set; }

            /// <summary>
            /// Gets the mobile phone number of the customer.
            /// </summary>
            [JsonProperty]
            public string MobilePhoneNumber { get; private set; }

            /// <summary>
            /// Gets the home phone number of the customer.
            /// </summary>
            [JsonProperty]
            public string HomePhoneNumber { get; private set; }

            /// <summary>
            /// Gets the work phone number of the customer.
            /// </summary>
            [JsonProperty]
            public string WorkPhoneNumber { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the customer user has been invited to activate.
            /// </summary>
            [JsonProperty]
            public bool UserHasBeenInvitedToActivate { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the customer user has been activated.
            /// </summary>
            [JsonProperty]
            public bool UserHasBeenActivated { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the person user is blocked.
            /// </summary>
            [JsonProperty]
            public bool UserIsBlocked { get; private set; }
        }
    }
}
