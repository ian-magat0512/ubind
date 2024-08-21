// <copyright file="CustomerImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class CustomerAggregate
    {
        /// <summary>
        /// The customer has to be imported.
        /// </summary>
        public class CustomerImportedEvent
            : Event<CustomerAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerImportedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="personData">The information of the person the customer represents.</param>
            /// <param name="environment">The data environment the customer is associated with.</param>
            /// <param name="performingUserId">The userId who imported customer.</param>
            /// <param name="portalId">The ID of the portal the customer would be expected to login to,
            /// if they end up having a user account.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            /// <param name="isTestData">A value indicating whether to return a test data.</param>
            public CustomerImportedEvent(
                Guid tenantId,
                PersonData personData,
                DeploymentEnvironment environment,
                Guid? performingUserId,
                Guid? portalId,
                Instant createdTimestamp,
                bool isTestData)
                : base(tenantId, Guid.NewGuid(), performingUserId, createdTimestamp)
            {
                this.Environment = environment;
                this.PersonData = personData;
                this.IsTestData = isTestData;
                this.PortalId = portalId;
            }

            [JsonConstructor]
            private CustomerImportedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the environment the person belongs to.
            /// </summary>
            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }

            /// <summary>
            /// Gets the information of the person this customer represents.
            /// </summary>
            [JsonProperty]
            public PersonData PersonData { get; private set; }

            /// <summary>
            /// Gets a value indicating whether to return a test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; private set; }

            /// <summary>
            /// Gets the ID of the portal which the customer would log into by default,
            /// If there is no specific portal required for a given product.
            /// This would be null if the customer doesn't log into a portal, or the customer
            /// is expected to login to the default portal for the tenanacy.
            /// This needed for the generation of links in emails, e.g. the user activation link.
            /// </summary>
            [JsonProperty]
            public Guid? PortalId { get; private set; }
        }
    }
}
