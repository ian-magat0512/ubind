// <copyright file="PersonImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person has been imported from customer details.
        /// </summary>
        public class PersonImportedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonImportedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the person belongs to.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="organisationId">The Id of the organisation the person belongs to.</param>
            /// <param name="data">The customer import data object.</param>
            /// <param name="performingUserId">The userId who imported customer.</param>
            /// <param name="createdTimestamp">The time the person aggregate was created.</param>
            public PersonImportedEvent(
                Guid tenantId,
                Guid personId,
                Guid organisationId,
                PersonData data,
                Guid? performingUserId,
                Instant createdTimestamp,
                bool isTestData)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.FullName = data.FullName;
                this.PreferredName = data.PreferredName;
                this.Email = data.Email;
                this.AlternativeEmail = data.AlternativeEmail;
                this.MobilePhone = data.MobilePhone;
                this.HomePhone = data.HomePhone;
                this.WorkPhone = data.WorkPhone;

                this.PersonData = data;
                this.IsTestData = isTestData;
            }

            [JsonConstructor]
            private PersonImportedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the tenant string Id.
            /// Note: for backward compatibility only.
            /// </summary>
            /// Remark: used for UB-7141 migration, you can remove right after.
            [JsonProperty("TenantId")]
            public string TenantStringId { get; private set; }

            /// <summary>
            /// Gets the Customer ID of the person.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the person belongs to.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the person's full name if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string FullName { get; private set; }

            /// <summary>
            /// Gets the person's preferred name if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string PreferredName { get; private set; }

            /// <summary>
            /// Gets the person's email address if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string Email { get; private set; }

            /// <summary>
            /// Gets the person's alternative email address if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string AlternativeEmail { get; private set; }

            /// <summary>
            /// Gets the person's mobile phone if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string MobilePhone { get; private set; }

            /// <summary>
            /// Gets the person's home phone if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string HomePhone { get; private set; }

            /// <summary>
            /// Gets the person's work phone if specified, otherwise null.
            /// </summary>
            /// <remarks>This has been made obsolete by PersonData property below.
            /// Property retained for backward compatibility.</remarks>
            [JsonProperty]
            public string WorkPhone { get; private set; }

            /// <summary>
            /// Gets the person's full data.
            /// </summary>
            /// <remarks>
            /// Only imported records since release of UB-6126 will have this property.
            /// Otherwise, data will be filled from older properties above.
            /// </remarks>
            [JsonProperty]
            public PersonData PersonData { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to return a test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; private set; }
        }
    }
}
