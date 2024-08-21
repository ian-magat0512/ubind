// <copyright file="Policy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using TimeZoneNames;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;
    using Transaction = UBind.Domain.ReadModel.Policy.PolicyTransaction;

    /// <summary>
    /// This class is needed because we need to generate json representation of policy entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Policy : EntityProducedByFormsApp<PolicyReadModel>
    {
        private DateTimeZone timeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Policy(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        /// <param name="model">The policy read model with related entities.</param>
        /// <param name="policyTransactions">The transactions related to the policy.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="configProvider">The product configuration provider.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        public Policy(
            PolicyReadModel model,
            IEnumerable<Transaction> policyTransactions,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties = default)
            : base(
                  model.Id,
                  model.CreatedTicksSinceEpoch,
                  model.LastModifiedTicksSinceEpoch,
                  formDataPrettifier,
                  includedProperties)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
            this.timeZone = model.TimeZone;

            var inceptionDateTimeVariants = new DateTimeVariants(model.InceptionDateTime, this.timeZone, model.InceptionTimestamp);
            this.InceptionTicksSinceEpoch = inceptionDateTimeVariants.TicksSinceEpoch;
            this.InceptionDateTime = inceptionDateTimeVariants.DateTime;
            this.InceptionDate = inceptionDateTimeVariants.Date;
            this.InceptionTime = inceptionDateTimeVariants.Time;
            this.InceptionTimeZoneName = inceptionDateTimeVariants.TimeZoneName;
            this.InceptionTimeZoneAbbreviation = inceptionDateTimeVariants.TimeZoneAbbreviation;

            var latestPolicyPeriodStartDateTimeVariants = new DateTimeVariants(model.LatestPolicyPeriodStartDateTime, this.timeZone, model.LatestPolicyPeriodStartTimestamp);
            this.LatestPolicyPeriodStartTicksSinceEpoch = latestPolicyPeriodStartDateTimeVariants.TicksSinceEpoch.Value;
            this.LatestPolicyPeriodStartDateTime = latestPolicyPeriodStartDateTimeVariants.DateTime;
            this.LatestPolicyPeriodStartDate = latestPolicyPeriodStartDateTimeVariants.Date;
            this.LatestPolicyPeriodStartTime = latestPolicyPeriodStartDateTimeVariants.Time;
            this.LatestPolicyPeriodStartTimeZoneName = latestPolicyPeriodStartDateTimeVariants.TimeZoneName;
            this.LatestPolicyPeriodStartTimeZoneAbbreviation = latestPolicyPeriodStartDateTimeVariants.TimeZoneAbbreviation;

            if (model.ExpiryTimestamp.HasValue)
            {
                var expiryDateTimeVariants = new DateTimeVariants(model.ExpiryDateTime.Value, this.timeZone, model.ExpiryTimestamp);
                this.ExpiryTicksSinceEpoch = expiryDateTimeVariants.TicksSinceEpoch;
                this.ExpiryDateTime = expiryDateTimeVariants.DateTime;
                this.ExpiryDate = expiryDateTimeVariants.Date;
                this.ExpiryTime = expiryDateTimeVariants.Time;
                this.ExpiryTimeZoneName = expiryDateTimeVariants.TimeZoneName;
                this.ExpiryTimeZoneAbbreviation = expiryDateTimeVariants.TimeZoneAbbreviation;
            }

            this.TenantId = model.TenantId.ToString();
            this.OwnerId = model.OwnerUserId.ToString();
            this.ProductId = model.ProductId.ToString();
            this.CustomerId = model.CustomerId.ToString();
            this.Environment = model.Environment.ToString().ToCamelCase();
            this.PolicyNumber = model.PolicyNumber ?? string.Empty;

            this.Status = this.GetPolicyStatus(model).ToString()?.ToCamelCase();
            if (includedProperties?.Any() == true)
            {
                var calculationResult = model.SerializedCalculationResult.IsNotNullOrEmpty() ? model.CalculationResult : null;
                var premium = calculationResult?.JObject?.SelectToken("payment.priceComponents")?.ToObject<JObject>() ?? new JObject();
                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.Premium))))
                {
                    this.Premium = new PremiumBreakdown(premium);
                }

                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.PremiumFormatted))))
                {
                    var currency = calculationResult?.PayablePrice?.CurrencyCode ?? "AUD";
                    this.PremiumFormatted = new PremiumBreakdownFormatted(premium, currency);
                }
            }

            var displayTransaction = this.GetDisplayTransaction(policyTransactions, model.AreTimestampsAuthoritative);
            if (displayTransaction != null)
            {
                this.SetFormData(displayTransaction?.PolicyData?.FormData);

                FormDataSchema? formDataSchema;
                var productContext = new ProductContext(
                    model.TenantId,
                    model.ProductId,
                    model.Environment);
                if (displayTransaction.ProductReleaseId != null)
                {
                    var context = new ReleaseContext(
                        model.TenantId,
                        model.ProductId,
                        model.Environment,
                        displayTransaction.ProductReleaseId.Value);
                    formDataSchema = configProvider.GetFormDataSchema(context, WebFormAppType.Quote);
                }
                else
                {
                    formDataSchema = configProvider.GetFormDataSchema(productContext, WebFormAppType.Quote);
                }

                this.SetFormDataFormatted(
                    displayTransaction?.PolicyData?.FormData, formDataSchema, nameof(Policy), productContext);
            }

            this.OrganisationId = model.OrganisationId.ToString();
            this.IsTestData = model.IsTestData;
            this.AggregateId = model.Id;

            this.EntityDescriptor = this.PolicyNumber;
            this.EntityReference = this.PolicyNumber;
            this.EntityEnvironment = model.Environment.ToString();

            this.TimeZoneId = model.TimeZone.ToString();
            var tzNames = TZNames.GetNamesForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneName = tzNames.Generic;
            var tzAbbreviations = TZNames.GetAbbreviationsForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneAbbreviation = tzAbbreviations.Generic;
        }

        public Policy(
            IPolicyReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
            : this(model.Policy, model.PolicyTransactions, formDataPrettifier, configProvider, includedProperties)
        {
            if (model.Customer != null)
            {
                this.Customer = new Customer(model.Customer);
            }

            if (model.Owner != null)
            {
                this.Owner = new User(model.Owner);
            }

            if (model.Product != null)
            {
                this.Product = new Product(model.Product, baseApiUrl, cachingResolver);
            }

            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.QuoteDocuments != null)
            {
                this.Documents = model.QuoteDocuments.Select(e => new Document(e, baseApiUrl)).ToList();
            }

            if (model.ClaimDocuments != null)
            {
                if (this.Documents == null)
                {
                    this.Documents = new List<Document>();
                }

                this.Documents.AddRange(model.ClaimDocuments.Select(e => new Document(e, baseApiUrl)).ToList());
            }

            if (model.Emails != null)
            {
                this.Messages = model.Emails.Select(e => new EmailMessage(e)).ToList<Message>();
            }

            if (model.Quotes != null)
            {
                this.Quotes = model.Quotes
                    .Select(e => new Quote(e, formDataPrettifier, configProvider))
                    .ToList();
            }

            if (model.Claims != null)
            {
                this.Claims = model.Claims
                    .Select(e => new Claim(e, formDataPrettifier, configProvider))
                    .ToList();
            }

            if (model.PolicyTransactions != null &&
                (includedProperties != null &&
                includedProperties.Any(x => x.EqualsIgnoreCase(nameof(Policy.PolicyTransactions)))))
            {
                this.PolicyTransactions = model.PolicyTransactions
                    .OrderByDescending(x => x.CreatedTicksSinceEpoch)
                    .Select(e => new PolicyTransaction(
                        e,
                        formDataPrettifier,
                        configProvider,
                        Enumerable.Empty<string>(),
                        model.Policy.TimeZone,
                        timeOfDayScheme))
                    .ToList();
            }

            if (model.Sms != null)
            {
                if (this.Messages == null)
                {
                    this.Messages = new List<Message>();
                }

                var smsMessages = model.Sms.Select(s => new SmsMessage(s)).ToList<Message>();
                this.Messages.AddRange(smsMessages);
            }

            if (model.FromRelationships != null)
            {
                this.Relationships = model.FromRelationships.Select(e => new Relationship(e)).ToList();
            }

            if (model.ToRelationships != null)
            {
                if (this.Relationships == null)
                {
                    this.Relationships = new List<Relationship>();
                }

                var relationships = model.ToRelationships.Select(e => new Relationship(e)).ToList();
                this.Relationships.AddRange(relationships);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        [JsonConstructor]
        private Policy()
        {
        }

        [JsonProperty(PropertyName = "timeZoneId", Order = 2)]
        public string TimeZoneId { get; set; }

        [JsonProperty(PropertyName = "timeZoneName", Order = 3)]
        public string TimeZoneName { get; set; }

        [JsonProperty(PropertyName = "timeZoneAbbreviation", Order = 4)]
        public string TimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the inception ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTicksSinceEpoch", Order = 21)]
        public long? InceptionTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionDateTime", Order = 22)]
        [Required]
        public string InceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionDate", Order = 23)]
        [Required]
        public string InceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the time of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTime", Order = 24)]
        [Required]
        public string InceptionTime { get; set; }

        /// <summary>
        /// Gets or sets the inception local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTimeZoneName", Order = 25)]
        [Required]
        public string InceptionTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the inception local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTimeZoneAbbreviation", Order = 26)]
        [Required]
        public string InceptionTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the expiry ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartTicksSinceEpoch", Order = 27)]
        public long LatestPolicyPeriodStartTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartDateTime", Order = 28)]
        [Required]
        public string LatestPolicyPeriodStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartDate", Order = 29)]
        [Required]
        public string LatestPolicyPeriodStartDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartTime", Order = 30)]
        [Required]
        public string LatestPolicyPeriodStartTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartTimeZoneName", Order = 31)]
        [Required]
        public string LatestPolicyPeriodStartTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "latestPolicyPeriodStartTimeZoneAbbreviation", Order = 32)]
        [Required]
        public string LatestPolicyPeriodStartTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the expiry ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "expiryTicksSinceEpoch", Order = 33)]
        public long? ExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDateTime", Order = 34)]
        [Required]
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDate", Order = 35)]
        [Required]
        public string ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTime", Order = 36)]
        [Required]
        public string ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZoneName", Order = 37)]
        [Required]
        public string ExpiryTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZoneAbbreviation", Order = 38)]
        [Required]
        public string ExpiryTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 44)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 45)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organisation Id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 46)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 47)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the owner user Id.
        /// </summary>
        [JsonProperty(PropertyName = "ownerId", Order = 48)]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the policy.
        /// </summary>
        [JsonProperty(PropertyName = "owner", Order = 49)]
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the product Id.
        /// </summary>
        [JsonProperty(PropertyName = "productId", Order = 50)]
        [Required]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "product", Order = 51)]
        public Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        [JsonProperty(PropertyName = "customerId", Order = 52)]
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "customer", Order = 53)]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        [JsonProperty(PropertyName = "environment", Order = 54)]
        [Required]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the status of the policy.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 55)]
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the policy number.
        /// </summary>
        [JsonProperty(PropertyName = "policyNumber", Order = 56)]
        [Required]
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        [JsonProperty(PropertyName = "premium", Order = 57)]
        public PremiumBreakdown Premium { get; set; }

        /// <summary>
        /// Gets or sets the formatted premium.
        /// </summary>
        [JsonProperty(PropertyName = "premiumFormatted", Order = 58)]
        public PremiumBreakdownFormatted PremiumFormatted { get; set; }

        /// <summary>
        /// Gets or sets the Ids of quotes associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "quoteIds", Order = 59)]
        public List<string> QuoteIds { get; set; }

        /// <summary>
        /// Gets or sets the quotes associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "quotes", Order = 60)]
        public List<Quote> Quotes { get; set; }

        /// <summary>
        /// Gets or sets the Ids of policy transactions associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactionIds", Order = 61)]
        public List<string> PolicyTransactionIds { get; set; }

        /// <summary>
        /// Gets or sets the policy transactions associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactions", Order = 62)]
        public List<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the Ids of claims associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "claimIds", Order = 63)]
        public List<string> ClaimIds { get; set; }

        /// <summary>
        /// Gets or sets the claims associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "claims", Order = 64)]
        public List<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets the messages associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 68)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 69)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the relationships associated with the policy.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 70)]
        public List<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }

        private PolicyStatus GetPolicyStatus(PolicyReadModel policy)
        {
            return this.GetPolicyStatus(
                policy.CancellationEffectiveTicksSinceEpoch,
                policy.ExpiryTicksSinceEpoch,
                policy.InceptionTicksSinceEpoch);
        }

        private PolicyStatus GetPolicyStatus(
            long? cancellationEffectiveTicksSinceEpoch,
            long? expiryTicksSinceEpoch,
            long inceptionTicksSinceEpoch)
        {
            // TODO: get the logic for this and PolicyReadModelDetails.GetDetailStatus and put it in one place
            // So it's dry, and consistent.
            var currentTicksSinceEpoch = SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks();
            if (cancellationEffectiveTicksSinceEpoch.HasValue && cancellationEffectiveTicksSinceEpoch < currentTicksSinceEpoch)
            {
                return PolicyStatus.Cancelled;
            }

            if (expiryTicksSinceEpoch.HasValue && expiryTicksSinceEpoch < currentTicksSinceEpoch)
            {
                return PolicyStatus.Expired;
            }

            if (inceptionTicksSinceEpoch > currentTicksSinceEpoch)
            {
                return PolicyStatus.Issued;
            }

            return PolicyStatus.Active;
        }

        private Transaction GetDisplayTransaction(
            IEnumerable<Transaction> policyTransactions,
            bool areTimestampsAuthoritative)
        {
            var nowTimestamp = SystemClock.Instance.GetCurrentInstant();

            // Return the latest transaction excluding cancellation transaction whose effective time has already been reached
            // or the latest transaction if none has been reached (i.e. new business transaction
            // for issued policy).
            return policyTransactions == null || !policyTransactions.Any() ?
                null :
                policyTransactions
                    .OfType<Transaction>()
                    .OrderBy(t => t.CreatedTicksSinceEpoch)
                    .Where(t => areTimestampsAuthoritative
                        ? t.EffectiveTimestamp < nowTimestamp
                        : t.EffectiveDateTime.InZoneLeniently(this.timeZone).ToInstant() < nowTimestamp)
                    .Where(t => t.GetType() != typeof(CancellationTransaction))
                    .LastOrDefault()
                ?? policyTransactions
                    .OfType<Transaction>()
                    .OrderBy(t => t.CreatedTicksSinceEpoch)
                    .Where(t => t.GetType() != typeof(CancellationTransaction))
                    .LastOrDefault();
        }
    }
}
