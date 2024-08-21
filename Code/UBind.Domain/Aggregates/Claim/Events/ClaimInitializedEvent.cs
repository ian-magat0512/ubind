// <copyright file="ClaimInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// A claim has been created.
        /// </summary>
        public class ClaimInitializedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the claim relates to.</param>
            /// <param name="organisationId">The ID of the organisation the claim relates to.</param>
            /// <param name="productId">The ID of the product the claim relates to.</param>
            /// <param name="environment">The environment the claim belongs to.</param>
            /// <param name="isTestData">The indicator whether claim is to be considered as isTestData.</param>
            /// <param name="claimId">A unique ID for the claim.</param>
            /// <param name="quoteId">The ID of the quote the claim pertains to.</param>
            /// <param name="referenceNumber">A reference number for the claim.</param>
            /// <param name="policyNumber">The number of the policy the claim is for.</param>
            /// <param name="customerId">The ID of the customer the claim is for.</param>
            /// <param name="personId">The ID of the person the claim is for.</param>
            /// <param name="customerFullName">The full name of the customer the claim is for.</param>
            /// <param name="customerPreferredName">The preferred name of the customer the claim is for.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ClaimInitializedEvent(
                Guid tenantId,
                Guid organisationId,
                Guid productId,
                DeploymentEnvironment environment,
                bool isTestData,
                Guid claimId,
                Guid? quoteId,
                string referenceNumber,
                string policyNumber,
                Guid? customerId,
                Guid? personId,
                string customerFullName,
                string customerPreferredName,
                Guid? performingUserId,
                Instant createdTimestamp,
                DateTimeZone timeZone)
                : base(tenantId, claimId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.ProductId = productId;
                this.Environment = environment;
                this.IsTestData = isTestData;
                this.PolicyId = quoteId;
                this.ReferenceNumber = referenceNumber;
                this.PolicyNumber = policyNumber;
                this.CustomerId = customerId;
                this.PersonId = personId;
                this.CustomerFullName = customerFullName;
                this.CustomerPreferredName = customerPreferredName;
                this.TimeZoneId = timeZone.ToString();
            }

            [JsonConstructor]
            private ClaimInitializedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the organisation the claim belongs to.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets a value indicating whether claim is to be considered as test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; private set; }

            /// <summary>
            /// Gets the ID of the product the claim belongs to.
            /// Note: For Backward compatibility with events, It is to be converted to
            /// JsonProperty("ProductNewId") is important.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; private set; }

            /// <summary>
            /// Gets the environment the claim belongs to.
            /// </summary>
            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }

            /// <summary>
            /// Gets the ID of the policy the claim pertains to, if any.
            /// </summary>
            /// Note the json property should never have been called "quoteId", but since it was done like this
            /// and we have all of these events persisted, we can't change it.
            [JsonProperty(PropertyName = "quoteId")]
            public Guid? PolicyId { get; private set; }

            /// <summary>
            /// Gets the reference number for the claim.
            /// </summary>
            [JsonProperty]
            public string ReferenceNumber { get; private set; }

            /// <summary>
            /// Gets the amount of the claim, if specified, otherwise null.
            /// </summary>
            /// <remarks>No longer used in new events, but may exist on old events.</remarks>
            [JsonProperty]
            public decimal? Amount { get; private set; }

            /// <summary>
            /// Gets the description of the claim if specified, otherwise null.
            /// </summary>
            /// <remarks>No longer used in new events, but may exist on old events.</remarks>
            [JsonProperty]
            public string Description { get; private set; }

            /// <summary>
            /// Gets the status of the claim.
            /// </summary>
            [JsonProperty]
            public string Status { get; private set; }

            /// <summary>
            /// Gets the date the incident being claimed for occurred, if specified, otherwise null.
            /// </summary>
            /// <remarks>No longer used in new events, but may exist on old events.</remarks>
            [JsonProperty]
            public LocalDate? IncidentDate { get; private set; }

            /// <summary>
            /// Gets the number of the policy the claim relates to.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            /// <summary>
            /// Gets the ID of the customer the claim is for.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the person the customer petains to.
            /// </summary>
            [JsonProperty]
            public Guid? PersonId { get; private set; }

            /// <summary>
            /// Gets the full name of the customer the claim is for.
            /// </summary>
            [JsonProperty]
            public string CustomerFullName { get; private set; }

            /// <summary>
            /// Gets the preferred name of the customer the claim is for.
            /// </summary>
            [JsonProperty]
            public string CustomerPreferredName { get; private set; }

            /// <summary>
            /// Gets the TZDB timezone ID, e.g. "Australia/Melbourne".
            /// </summary>
            [JsonProperty]
            public string TimeZoneId { get; private set; }
        }
    }
}
