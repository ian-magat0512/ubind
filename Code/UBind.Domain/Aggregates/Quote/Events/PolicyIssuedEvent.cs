// <copyright file="PolicyIssuedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote has been created.
        /// </summary>
        public class PolicyIssuedEvent : Event<QuoteAggregate, Guid>, IPolicyUpsertEvent, IPolicyCreatedEvent
        {
            private Guid? quoteId;
            private Guid newBusinessTransactionId;
            private ReadWriteModel.CalculationResult calculationResult;
            private LocalDateTime? inceptionDateTime;
            private LocalDateTime? expiryDateTime;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyIssuedEvent"/> class.
            /// </summary>
            /// <param name="quoteAggregate">The quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="quoteNumber">The quote number of the quote for the policy.</param>
            /// <param name="policyNumber">The policy number.</param>
            /// <param name="calculationResult">The calculation result for the policy.</param>
            /// <param name="inceptionDateTime">The date the policy begins.</param>
            /// <param name="expiryDateTime">The date the policy expires.</param>
            /// <param name="dataSnapshot">The quote data that was used in the policy.</param>
            /// <param name="performingUserId">The userId who issued the policy.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PolicyIssuedEvent(
                QuoteAggregate quoteAggregate,
                Guid quoteId,
                string quoteNumber,
                string policyNumber,
                ReadWriteModel.CalculationResult calculationResult,
                LocalDateTime inceptionDateTime,
                Instant inceptionTimestamp,
                LocalDateTime expiryDateTime,
                Instant expiryTimestamp,
                DateTimeZone timeZone,
                QuoteDataSnapshot dataSnapshot,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId)
                : base(quoteAggregate.TenantId, quoteAggregate.Id, performingUserId, createdTimestamp)
            {
                timeZone.Should().NotBeNull();
                this.ProductId = quoteAggregate.ProductId;
                this.Environment = quoteAggregate.Environment;
                this.IsTestData = quoteAggregate.IsTestData;
                this.QuoteId = quoteId;
                this.QuoteNumber = quoteNumber;
                this.PolicyNumber = policyNumber;
                this.CalculationResult = calculationResult;
                this.InceptionTimestamp = inceptionTimestamp;
                this.InceptionDateTime = inceptionDateTime;
                this.ExpiryDateTime = expiryDateTime;
                this.ExpiryTimestamp = expiryTimestamp;
                this.DataSnapshot = dataSnapshot;
                this.NewBusinessTransactionId = Guid.NewGuid();
                this.TimeZoneId = timeZone.ToString();
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private PolicyIssuedEvent()
            {
            }

            /// <summary>
            /// Gets or sets an ID identifying which product the quote is for.
            /// Note: For Backward compatibility with events, It is to be converted
            /// JsonProperty("ProductNewId") is important.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to return a test data.
            /// </summary>
            [JsonProperty]
            public bool IsTestData { get; set; }

            /// <summary>
            /// Gets or sets the environment the quote belongs to.
            /// </summary>
            public DeploymentEnvironment Environment { get; set; }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid? QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.quoteId.GetValueOrDefault() == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets the quote number of the quote for the policy.
            /// </summary>
            [JsonProperty]
            public string QuoteNumber { get; private set; }

            /// <summary>
            /// Gets the policy number.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

#pragma warning disable CS0618 // Type or member is obsolete
            [JsonProperty]
            public LocalDateTime InceptionDateTime
            {
                // This uses the inceptionDateTime property if available, but has backwards compatibility
                // to support the obsolete inceptionDate and inceptionTime properties.
                get => this.inceptionDateTime.HasValue
                    ? this.inceptionDateTime.Value
                    : this.InceptionTimestamp != default
                        ? this.InceptionTimestamp.InZone(Timezones.AET).LocalDateTime
                        : this.InceptionDate.GetInstantAt4pmAet().InZone(Timezones.AET).LocalDateTime;
                set => this.inceptionDateTime = value;
            }

            [JsonProperty]
            public LocalDateTime? ExpiryDateTime
            {
                // This uses the expiryDateTime property if available, but has backwards compatibility
                // to support the obsolete ExpiryDate and ExpiryTime properties.
                get => this.expiryDateTime.HasValue
                    ? this.expiryDateTime.Value
                    : this.ExpiryTimestamp.HasValue && this.ExpiryTimestamp.Value != default
                        ? this.ExpiryTimestamp.Value.InZone(Timezones.AET).LocalDateTime
                        : this.ExpiryDate?.GetInstantAt4pmAet().InZone(Timezones.AET).LocalDateTime;
                set => this.expiryDateTime = value;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            /// <summary>
            /// Gets the calculation result for the policy.
            /// </summary>
            [JsonProperty]
            public ReadWriteModel.CalculationResult CalculationResult
            {
                get
                {
                    // Calculation result was not included in event prior to introduction
                    // of adjustments and renewals, but can be calculated for these
                    // new business policies from existing data.
                    return this.calculationResult
                        ?? ReadWriteModel.CalculationResult.CreateFromExistingPolicy(this);
                }

                private set
                {
                    this.calculationResult = value;
                }
            }

            /// <summary>
            /// Gets the inception date.
            /// </summary>
            [JsonProperty]
            [Obsolete("Use InceptionDateTime instead")]
            public LocalDate InceptionDate { get; private set; }

            /// <summary>
            /// Gets the expiry date.
            /// </summary>
            [JsonProperty]
            [Obsolete("Use ExpiryDateTime instead")]
            public LocalDate? ExpiryDate { get; private set; }

            /// <summary>
            /// Gets the inception date.
            /// </summary>
            [JsonProperty("inceptionTime")]
            public Instant InceptionTimestamp { get; set; }

            /// <summary>
            /// Gets the expiry date.
            /// </summary>
            [JsonProperty("expiryTime")]
            public Instant? ExpiryTimestamp { get; set; }

            /// <summary>
            /// Gets the form data used for the policy.
            /// </summary>
            [JsonProperty]
            public QuoteDataSnapshot DataSnapshot { get; private set; }

            /// <summary>
            /// Gets the policy data.
            /// </summary>
            [JsonIgnore]
            [Obsolete("We're just storing the DataSnapshot directly from now on")]
            public PolicyData PolicyData => new PolicyData(
                this.DataSnapshot);

            /// <summary>
            /// Gets the new business transaction id.
            /// </summary>
            [JsonProperty]
            public Guid NewBusinessTransactionId
            {
                get
                {
                    // This property does not exist on pre-existing policy issued events,
                    // we use the aggregate ID instead.
                    return this.newBusinessTransactionId == default
                        ? this.AggregateId
                        : this.newBusinessTransactionId;
                }

                private set
                {
                    this.newBusinessTransactionId = value;
                }
            }

            [JsonProperty]
            public string TimeZoneId { get; private set; }

            [JsonIgnore]
            public LocalDateTime EffectiveDateTime => this.InceptionDateTime;

            [JsonIgnore]
            public Instant EffectiveTimestamp => this.InceptionTimestamp;

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
