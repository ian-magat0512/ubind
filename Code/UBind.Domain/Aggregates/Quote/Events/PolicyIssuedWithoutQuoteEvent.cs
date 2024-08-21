// <copyright file="PolicyIssuedWithoutQuoteEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Services;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a policy without a quote has been created.
        /// </summary>
        public class PolicyIssuedWithoutQuoteEvent : Event<QuoteAggregate, Guid>, IPolicyUpsertEvent, IPolicyCreatedEvent
        {
            private Guid? quoteId;
            private Guid newBusinessTransactionId;
            private ReadWriteModel.CalculationResult calculationResult;
            private LocalDateTime? inceptionDateTime;
            private LocalDateTime? expiryDateTime;

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyIssuedWithoutQuoteEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant ID of the quote aggregate.</param>
            /// <param name="aggregateId">The quote aggregate ID.</param>
            /// <param name="organisationId">The organisation Id of the quote aggregate.</param>
            /// <param name="productId">The product ID of the quote aggregate.</param>
            /// <param name="environment">The environment of the quote aggregate.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="customerId">The ID of the customer.</param>
            /// <param name="personalDetails">The personal details of the quote.</param>
            /// <param name="policyNumber">The policy number.</param>
            /// <param name="inceptionDate">The policy inception date.</param>
            /// <param name="expiryDate">The policy expiry date.</param>
            /// <param name="formData">The form data of the policy.</param>
            /// <param name="calculationResult">The calculation result of the policy.</param>
            /// <param name="timeZone">The time zone.</param>
            /// <param name="timeOfDayScheme">Helper for setting inception and expiry times.</param>
            /// <param name="performingUserId">The userId who imported policy.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public PolicyIssuedWithoutQuoteEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                Guid productId,
                DeploymentEnvironment environment,
                Guid? personId,
                Guid? customerId,
                IPersonalDetails? personalDetails,
                string policyNumber,
                string policyTitle,
                LocalDate inceptionDate,
                LocalDate? expiryDate,
                FormData formData,
                JObject calculationResult,
                bool isTestData,
                DateTimeZone timeZone,
                IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
                Guid? performingUserId,
                Instant createdTimestamp,
                Guid? productReleaseId)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.IsTestData = isTestData;
                this.ProductId = productId;
                this.PersonId = personId;
                this.CustomerId = customerId;
                this.OrganisationId = organisationId;
                this.Environment = environment;
                this.PolicyNumber = policyNumber;
                this.PolicyTitle = policyTitle;
                this.InceptionDateTime = inceptionDate.At(timeOfDayScheme.GetInceptionTime());
                this.InceptionTimestamp = this.InceptionDateTime.InZoneLeniently(timeZone).ToInstant();
                this.ExpiryDateTime = expiryDate.HasValue ? expiryDate.Value.At(timeOfDayScheme.GetEndTime()) : null;
                this.ExpiryTimestamp = this.ExpiryDateTime?.InZoneLeniently(timeZone).ToInstant();

                var quoteDataRetriever = new StandardQuoteDataRetriever(new CachingJObjectWrapper(formData.JObject), new CachingJObjectWrapper(calculationResult));
                var calculationResultModel = ReadWriteModel.CalculationResult
                    .CreateForNewPolicy(new CachingJObjectWrapper(calculationResult), quoteDataRetriever);

                var dataSnapshot = new QuoteDataSnapshot(
                    new QuoteDataUpdate<FormData>(
                        aggregateId, formData, createdTimestamp),
                    new QuoteDataUpdate<ReadWriteModel.CalculationResult>(aggregateId, calculationResultModel, createdTimestamp),
                    personalDetails != null ? new QuoteDataUpdate<IPersonalDetails>(aggregateId, personalDetails, createdTimestamp) : null);

                this.DataSnapshot = dataSnapshot;
                this.CalculationResult = calculationResultModel;
                this.NewBusinessTransactionId = Guid.NewGuid();
                this.TimeZoneId = timeZone.ToString();
                this.ProductReleaseId = productReleaseId;
            }

            [JsonConstructor]
            private PolicyIssuedWithoutQuoteEvent()
            {
            }

            /// <summary>
            /// Gets or sets the ID of the organisation for which the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; set; }

            /// <summary>
            /// Gets or sets the ID identifying which product the quote is for.
            /// Note: For Backward compatibility with events, It is to be converted to
            /// JsonProperty("ProductNewId") is important.
            /// </summary>
            [JsonProperty("ProductNewId")]
            public Guid ProductId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the tenant for which the quote is for.
            /// </summary>
            [JsonProperty]
            public Guid? PersonId { get; set; }

            /// <summary>
            /// Gets or sets an ID identifying which product the quote is for.
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
            /// Gets the policy number.
            /// </summary>
            [JsonProperty]
            public string PolicyNumber { get; private set; }

            public string PolicyTitle { get; private set; }

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
            public PolicyData PolicyData => new PolicyData(this.DataSnapshot);

            /// <summary>
            /// Gets the new business transaction id.
            /// </summary>
            [JsonProperty]
            public Guid NewBusinessTransactionId
            {
                get => this.newBusinessTransactionId =
                    this.newBusinessTransactionId == default ? Guid.NewGuid() : this.newBusinessTransactionId;
                private set => this.newBusinessTransactionId = value;
            }

            [JsonProperty]
            public string TimeZoneId { get; }

            [JsonIgnore]
            public LocalDateTime EffectiveDateTime => this.InceptionDateTime;

            [JsonIgnore]
            public Instant EffectiveTimestamp => this.InceptionTimestamp;

            [JsonProperty]
            public Guid? ProductReleaseId { get; private set; }
        }
    }
}
