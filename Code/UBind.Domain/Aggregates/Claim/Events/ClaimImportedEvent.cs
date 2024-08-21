// <copyright file="ClaimImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// A claim has been imported.
        /// </summary>
        public class ClaimImportedEvent : Event<ClaimAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimImportedEvent"/> class.
            /// </summary>
            /// <param name="referenceNumber">A random unique reference number for the claim.</param>
            /// <param name="policy">The policy read model of the claim relates to.</param>
            /// <param name="person">The person aggregate the claim is for.</param>
            /// <param name="data">The claim imported data object.</param>
            /// <param name="performingUserId">The userId who import.</param>
            /// <param name="timestamp">The time the person aggregate was notified.</param>
            /// <param name="timeZone">We need to store the timezone for the user who created the claim, so that when
            /// we show them the dates, we can show them in their local time.</param>
            public ClaimImportedEvent(
                string referenceNumber,
                PolicyReadModel policy,
                PersonAggregate person,
                ClaimImportData data,
                Guid? performingUserId,
                Instant timestamp,
                DateTimeZone timeZone)
                : base(policy.TenantId, Guid.NewGuid(), performingUserId, timestamp)
            {
                this.OrganisationId = policy.OrganisationId;
                this.ProductId = policy.ProductId;
                this.Environment = policy.Environment;
                this.PolicyId = policy.Id;
                this.Status = data.Status;
                this.PolicyNumber = data.PolicyNumber;
                this.ClaimNumber = data.ClaimNumber;
                this.ReferenceNumber = referenceNumber;
                this.Amount = data.Amount;
                this.Description = data.Description;
                this.IncidentDate = data.IncidentDate.ToLocalDateFromMdyy();
                this.CustomerId = policy.CustomerId;
                this.PersonId = person.Id;
                this.CustomerFullName = person.FullName;
                this.CustomerPreferredName = person.PreferredName;
                this.TimeZoneId = timeZone.ToString();
            }

            [JsonConstructor]
            private ClaimImportedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the organisation the claim belongs to.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

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
            /// Gets the ID of the policy the claim pertains to.
            /// </summary>
            [JsonProperty]
            public Guid PolicyId { get; private set; }

            /// <summary>
            /// Gets the state for the claim.
            /// </summary>
            /// <remarks>
            /// Previously this event included a Status property of type <see cref="LegacyClaimStatus"/> that was always set
            /// to New. That property is now ignored (since the json property name has been changed to "state") and
            /// this string property is used instead. If this property is not set, the claim should be set to a default
            /// state (currently "Incomplete").
            /// </remarks>
            [JsonProperty(PropertyName = "state")]
            public string Status { get; private set; }

            /// <summary>
            /// Gets the number of the policy the claim relates to.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            /// <summary>
            /// Gets the claim number for the claim.
            /// </summary>
            /// <remarks>Named referenceNumber for backwards compatibility, but is really the claim number, not the reference number..</remarks>
            [JsonProperty(PropertyName = "referenceNumber")]
            public string ClaimNumber { get; private set; }

            /// <summary>
            /// Gets the claim number for the claim.
            /// </summary>
            /// <remarks>Named realReferenceNumber for backwards compatibility (see <see cref="ClaimNumber" />).</remarks>
            [JsonProperty(PropertyName = "realReferenceNumber")]
            public string ReferenceNumber { get; private set; }

            /// <summary>
            /// Gets the amount of the claim.
            /// </summary>
            [JsonProperty]
            public decimal Amount { get; private set; }

            /// <summary>
            /// Gets the description of the claim.
            /// </summary>
            [JsonProperty]
            public string Description { get; private set; }

            /// <summary>
            /// Gets the date the incident being claimed for occurred.
            /// </summary>
            [JsonProperty]
            public LocalDate? IncidentDate { get; private set; }

            /// <summary>
            /// Gets the ID of the customer the claim is for.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the person the customer petains to.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

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
