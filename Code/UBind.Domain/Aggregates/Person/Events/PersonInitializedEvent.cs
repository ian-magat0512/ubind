// <copyright file="PersonInitializedEvent.cs" company="uBind">
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
        /// The person has been initialized.
        /// </summary>
        public class PersonInitializedEvent
            : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the Person's tenant.</param>
            /// <param name="personId">A unique ID for the Person.</param>
            /// <param name="customerId">The Customer ID of the person to be created.</param>
            /// <param name="userId">The User ID of the person to be created.</param>
            /// <param name="personData">The person data.</param>
            /// <param name="organisationId">The ID of the person's organisation.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PersonInitializedEvent(
                Guid tenantId,
                Guid personId,
                Guid? customerId,
                Guid? userId,
                PersonData personData,
                Guid organisationId,
                Guid? performingUserId,
                Instant createdTimestamp,
                bool isTestData)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.CustomerId = customerId;
                this.UserId = userId;
                this.PersonData = personData;
                this.OrganisationId = organisationId;
                this.IsTestData = isTestData;
            }

            [JsonConstructor]
            private PersonInitializedEvent()
                : base(default(Guid), default(Guid), default(Guid), default(Instant))
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
            /// Gets the ID of the Customer.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the User.
            /// </summary>
            [JsonProperty]
            public Guid? UserId { get; private set; }

            /// <summary>
            /// Gets the information of the person this data represents.
            /// </summary>
            [JsonProperty]
            public PersonData PersonData { get; private set; }

            /// <summary>
            /// Gets the ID of the Person's organisation.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to return a test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; private set; }
        }
    }
}
