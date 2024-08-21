// <copyright file="AggregateCreationFromPolicyEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a policy has been created.
        /// This event will never happen after the migration, this only fix anomally data.
        /// </summary>
        public class AggregateCreationFromPolicyEvent : Event<QuoteAggregate, Guid>, IPolicyUpsertEvent
        {
            private Guid? quoteId;
            private ReadWriteModel.CalculationResult calculationResult;
            private LocalDateTime? inceptionDateTime;
            private LocalDateTime? expiryDateTime;

            public AggregateCreationFromPolicyEvent(
                Guid tenantId,
                Guid organisationId,
                Guid productId,
                DeploymentEnvironment environment,
                Guid aggregateId,
                Guid personId,
                Guid customerId,
                IPersonalDetails personalDetails,
                string formData,
                string calculationResult,
                string policyNumber,
                LocalDateTime inceptionDateTime,
                Instant inceptionTimestamp,
                LocalDateTime? expiryDateTime,
                Instant? expiryTimestamp,
                DateTimeZone timeZone,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.ProductId = productId;
                this.PersonId = personId;
                this.CustomerId = customerId;
                this.OrganisationId = organisationId;
                this.Environment = environment;
                this.PolicyNumber = policyNumber;
                this.InceptionDateTime = inceptionDateTime;
                this.InceptionTimestamp = inceptionTimestamp;
                this.ExpiryDateTime = expiryDateTime;
                this.ExpiryTimestamp = expiryTimestamp;
                this.TimeZoneId = timeZone.ToString();

                var quoteDataRetriever = new StandardQuoteDataRetriever(
                    new CachingJObjectWrapper(formData),
                    new CachingJObjectWrapper(calculationResult));
                var calculationResultModel = ReadWriteModel.CalculationResult
                    .CreateForNewPolicy(
                    new CachingJObjectWrapper(calculationResult),
                    quoteDataRetriever);

                var dataSnapshot = new QuoteDataSnapshot(
                    new QuoteDataUpdate<FormData>(
                        aggregateId,
                        new FormData(formData),
                        createdTimestamp),
                    new QuoteDataUpdate<ReadWriteModel.CalculationResult>(aggregateId, calculationResultModel, createdTimestamp),
                    new QuoteDataUpdate<IPersonalDetails>(aggregateId, personalDetails, createdTimestamp));

                this.DataSnapshot = dataSnapshot;
                this.CalculationResult = calculationResultModel;
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private AggregateCreationFromPolicyEvent()
            {
            }

            /// <summary>
            /// Gets or sets the ID of the organisation for which the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; set; }

            /// <summary>
            /// Gets or sets an guid ID identifying which product the quote is for.
            /// Note: this is json property "ProductNewId" since its already created this way,
            /// for backward compatibility.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the tenant for which the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the customer.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; set; }

            /// <summary>
            /// Gets the type of quote.
            /// </summary>
            [JsonProperty]
            public QuoteType Type { get; private set; }

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
                    return this.quoteId.GetValueOrDefault() == Guid.Empty
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets the policy number.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

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
            public Instant InceptionTimestamp { get; private set; }

            /// <summary>
            /// Gets the expiry date.
            /// </summary>
            [JsonProperty("expiryTime")]
            public Instant? ExpiryTimestamp { get; private set; }

            [JsonProperty]
            public string TimeZoneId { get; }

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
            public PolicyData PolicyData => new PolicyData(this.DataSnapshot);

            [JsonIgnore]
            public LocalDateTime EffectiveDateTime => this.InceptionDateTime;

            [JsonIgnore]
            public Instant EffectiveTimestamp => this.InceptionTimestamp;

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
