// <copyright file="PolicyTransaction.cs" company="uBind">
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
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;
    using Transaction = UBind.Domain.ReadModel.Policy.PolicyTransaction;

    /// <summary>
    /// This class is needed because we need to generate json representation of policy transaction entity
    /// that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class PolicyTransaction : EntityProducedByFormsApp<Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public PolicyTransaction(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="model">The policy transaction read model with related entities.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="configProvider">The product configuration provider.</param>
        /// <param name="timeOfDayScheme">The default policy transaction time of day scheme.</param>
        public PolicyTransaction(
            Transaction model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            DateTimeZone timeZone,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
            : base(
                  model.Id,
                  model.CreatedTicksSinceEpoch,
                  model.LastModifiedTicksSinceEpoch,
                  formDataPrettifier,
                  includedProperties)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
            var zone = Timezones.GetTimeZoneByState(DefaultState);

            var transaction = model;

            var effectiveDateTimeVariants = new DateTimeVariants(transaction.EffectiveDateTime, timeZone, transaction.EffectiveTimestamp);
            this.EffectiveDateTime = effectiveDateTimeVariants.DateTime;
            this.EffectiveTicksSinceEpoch = effectiveDateTimeVariants.TicksSinceEpoch;
            this.EffectiveDate = effectiveDateTimeVariants.Date;
            this.EffectiveTime = effectiveDateTimeVariants.Time;
            this.EffectiveTimeZoneName = effectiveDateTimeVariants.TimeZoneName;
            this.EffectiveTimeZoneAbbreviation = effectiveDateTimeVariants.TimeZoneAbbreviation;

            if (transaction.ExpiryDateTime.HasValue)
            {
                var expiryDateTimeVariants = new DateTimeVariants(transaction.ExpiryDateTime.Value, timeZone, transaction.ExpiryTimestamp);
                this.ExpiryDateTime = expiryDateTimeVariants.DateTime;
                this.ExpiryTicksSinceEpoch = expiryDateTimeVariants.TicksSinceEpoch;
                this.ExpiryDate = expiryDateTimeVariants.Date;
                this.ExpiryTime = expiryDateTimeVariants.Time;
                this.ExpiryTimeZoneName = expiryDateTimeVariants.TimeZoneName;
                this.ExpiryTimeZoneAbbreviation = expiryDateTimeVariants.TimeZoneAbbreviation;
            }

            if (transaction is NewBusinessTransaction newBusinessTransaction)
            {
                var inceptionDateTimeVariants = new DateTimeVariants(
                    newBusinessTransaction.InceptionDateTime, timeZone, newBusinessTransaction.InceptionTimestamp);
                this.InceptionDateTime = inceptionDateTimeVariants.DateTime;
                this.InceptionTicksSinceEpoch = inceptionDateTimeVariants.TicksSinceEpoch;
                this.InceptionDate = inceptionDateTimeVariants.Date;
                this.InceptionTime = inceptionDateTimeVariants.Time;
                this.InceptionTimeZoneName = inceptionDateTimeVariants.TimeZoneName;
                this.InceptionTimeZoneAbbreviation = inceptionDateTimeVariants.TimeZoneAbbreviation;
            }

            if (transaction is CancellationTransaction)
            {
                this.CancellationEffectiveDateTime = effectiveDateTimeVariants.DateTime;
                this.CancellationEffectiveTicksSinceEpoch = effectiveDateTimeVariants.TicksSinceEpoch;
                this.CancellationEffectiveDate = effectiveDateTimeVariants.Date;
                this.CancellationEffectiveTime = effectiveDateTimeVariants.Time;
                this.CancellationEffectiveTimeZoneName = effectiveDateTimeVariants.TimeZoneName;
                this.CancellationEffectiveTimeZoneAbbreviation = effectiveDateTimeVariants.TimeZoneAbbreviation;
            }

            if (transaction is AdjustmentTransaction)
            {
                this.AdjustmentEffectiveDateTime = effectiveDateTimeVariants.DateTime;
                this.AdjustmentEffectiveTicksSinceEpoch = effectiveDateTimeVariants.TicksSinceEpoch;
                this.AdjustmentEffectiveDate = effectiveDateTimeVariants.Date;
                this.AdjustmentEffectiveTime = effectiveDateTimeVariants.Time;
                this.AdjustmentEffectiveTimeZoneName = effectiveDateTimeVariants.TimeZoneName;
                this.AdjustmentEffectiveTimeZoneAbbreviation = effectiveDateTimeVariants.TimeZoneAbbreviation;
            }

            this.SetFormData(model.PolicyData?.FormData);

            FormDataSchema? formDataSchema;
            var productContext = new ProductContext(
                model.TenantId,
                model.ProductId,
                model.Environment);
            if (transaction.ProductReleaseId != null)
            {
                var context = new ReleaseContext(
                    model.TenantId,
                    model.ProductId,
                    model.Environment,
                    transaction.ProductReleaseId.Value);
                formDataSchema = configProvider.GetFormDataSchema(context, WebFormAppType.Quote);
            }
            else
            {
                formDataSchema = configProvider.GetFormDataSchema(productContext, WebFormAppType.Quote);
            }

            this.SetFormDataFormatted(model.PolicyData?.FormData, formDataSchema, nameof(PolicyTransaction), productContext);
            this.QuoteId = model.QuoteId.ToString();
            this.PolicyTransactionType = this.GetTransactionType(model).ToCamelCase();
            this.Environment = model.Environment.ToString().ToCamelCase();
            this.TenantId = model.TenantId.ToString();
            this.OwnerId = model.OwnerUserId.ToString();
            this.ProductId = model.ProductId.ToString();
            this.CustomerId = model.CustomerId.ToString();
            this.PolicyId = model.PolicyId.ToString();
            this.Status = model.GetTransactionStatus(timeOfDayScheme.AreTimestampsAuthoritative, timeZone).ToCamelCase();
            this.IsTestData = model.IsTestData;

            this.OrganisationId = model.OrganisationId.ToString();
            this.AggregateId = model.PolicyId;

            this.EntityDescriptor = this.Policy?.PolicyNumber;
            this.EntityEnvironment = model.Environment.ToString();
            if (includedProperties?.Any() == true)
            {
                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.Calculation))))
                {
                    this.Calculation = new QuoteCalculation(model.PolicyData?.SerializedCalculationResult);
                }

                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.CalculationFormatted))))
                {
                    this.CalculationFormatted = new QuoteCalculation(model.PolicyData?.SerializedCalculationResult, true);
                }
            }
        }

        public PolicyTransaction(
            IPolicyTransactionReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
            : this(model.PolicyTransaction, formDataPrettifier, configProvider, includedProperties, model.TimeZone, timeOfDayScheme)
        {
            if (model.Customer != null)
            {
                this.Customer = new Customer(model.Customer);
            }

            if (model.Policy != null)
            {
                this.Policy = new Policy(
                    model.Policy,
                    model.PolicyTransactions,
                    formDataPrettifier,
                    configProvider);
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

            if (model.Quote != null)
            {
                this.Quote = new Quote(
                    model.Quote,
                    formDataPrettifier,
                    configProvider);
            }

            if (model.Documents != null)
            {
                this.Documents = model.Documents.Select(e => new Document(e, baseApiUrl)).ToList();
            }

            if (model.Emails != null)
            {
                this.Messages = model.Emails.Select(e => new EmailMessage(e)).ToList<Message>();
            }

            if (model.Sms != null)
            {
                if (this.Messages == null)
                {
                    this.Messages = new List<Message>();
                }

                var smsMessages = model.Sms.Select(e => new SmsMessage(e)).ToList<Message>();
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
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        [JsonConstructor]
        private PolicyTransaction()
        {
        }

        /// <summary>
        /// Gets or sets the inception ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTicksSinceEpoch", Order = 21)]
        public long? InceptionTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionDateTime", Order = 22)]
        public string InceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionDate", Order = 23)]
        public string InceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the time of inception.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTime", Order = 24)]
        public string InceptionTime { get; set; }

        /// <summary>
        /// Gets or sets the inception local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTimeZone", Order = 25)]
        public string InceptionTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the inception local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "inceptionTimeZoneAlias", Order = 26)]
        public string InceptionTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the expiry ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "expiryTicksSinceEpoch", Order = 31)]
        public long? ExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDateTime", Order = 32)]
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDate", Order = 33)]
        public string ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTime", Order = 34)]
        public string ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZone", Order = 35)]
        public string ExpiryTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZoneAlias", Order = 36)]
        public string ExpiryTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the adjustment ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentTicksSinceEpoch", Order = 41)]
        public long? AdjustmentEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time of adjustment.
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentDateTime", Order = 42)]
        public string AdjustmentEffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date of adjustment.
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentDate", Order = 43)]
        public string AdjustmentEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the time of adjustment.
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentTime", Order = 44)]
        public string AdjustmentEffectiveTime { get; set; }

        /// <summary>
        /// Gets or sets the adjustment local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentTimeZone", Order = 45)]
        public string AdjustmentEffectiveTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the inception local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "adjustmentTimeZoneAlias", Order = 46)]
        public string AdjustmentEffectiveTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the cancellation ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "cancellationTicksSinceEpoch", Order = 51)]
        public long? CancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be cancelled.
        /// </summary>
        [JsonProperty(PropertyName = "cancellationDateTime", Order = 52)]
        public string CancellationEffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be cancelled.
        /// </summary>
        [JsonProperty(PropertyName = "cancellationDate", Order = 53)]
        public string CancellationEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be cancelled.
        /// </summary>
        [JsonProperty(PropertyName = "cancellationTime", Order = 54)]
        public string CancellationEffectiveTime { get; set; }

        /// <summary>
        /// Gets or sets the cancellation local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "cancellationTimeZone", Order = 55)]
        public string CancellationEffectiveTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the cancellation local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "cancellationTimeZoneAlias", Order = 56)]
        public string CancellationEffectiveTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the expiry ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "effectiveTicksSinceEpoch", Order = 61)]
        [Required]
        public long? EffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "effectiveDateTime", Order = 62)]
        [Required]
        public string EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "effectiveDate", Order = 63)]
        [Required]
        public string EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "effectiveTime", Order = 64)]
        [Required]
        public string EffectiveTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "effectiveTimeZone", Order = 65)]
        [Required]
        public string EffectiveTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "effectiveTimeZoneAlias", Order = 66)]
        [Required]
        public string EffectiveTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 67)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 68)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organisation Id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 69)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 70)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the owner user Id.
        /// </summary>
        [JsonProperty(PropertyName = "ownerId", Order = 71)]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "owner", Order = 72)]
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the product Id.
        /// </summary>
        [JsonProperty(PropertyName = "productId", Order = 72)]
        [Required]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "product", Order = 74)]
        public Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        [JsonProperty(PropertyName = "customerId", Order = 75)]
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "customer", Order = 76)]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the policy Id.
        /// </summary>
        [JsonProperty(PropertyName = "policyId", Order = 77)]
        [Required]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "policy", Order = 78)]
        public Policy Policy { get; set; }

        /// <summary>
        /// Gets or sets the quote Id.
        /// </summary>
        [JsonProperty(PropertyName = "quoteId", Order = 79)]
        [Required]
        public string QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the quote associated with the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "quote", Order = 80)]
        public Quote Quote { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        [JsonProperty(PropertyName = "environment", Order = 81)]
        [Required]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the transaction type.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactionType", Order = 82)]
        [Required]
        public string PolicyTransactionType { get; set; }

        /// <summary>
        /// Gets or sets the status of the policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 83)]
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "calculation", Order = 84)]
        public QuoteCalculation Calculation { get; set; }

        /// <summary>
        /// Gets or sets the results from the most recent calculation performed for this policy transaction,
        /// with values that are formatted based on their data type.
        /// </summary>
        [JsonProperty(PropertyName = "calculationFormatted", Order = 85)]
        public QuoteCalculation CalculationFormatted { get; set; }

        /// <summary>
        /// Gets or sets the invoice Id.
        /// </summary>
        [JsonProperty(PropertyName = "invoiceId", Order = 86)]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the credit note Id.
        /// </summary>
        [JsonProperty(PropertyName = "creditNoteId", Order = 87)]
        public string CreditNoteId { get; set; }

        /// <summary>
        /// Gets or sets the messages associated with policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 91)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 92)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the relationships associated with policy transaction.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 93)]
        public List<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }

        private string GetTransactionType(Transaction transaction)
        {
            switch (transaction)
            {
                case NewBusinessTransaction a:
                    return "newBusiness";
                case AdjustmentTransaction b:
                    return "adjustment";
                case RenewalTransaction c:
                    return "renewal";
                case CancellationTransaction d:
                    return "cancellation";
                default:
                    return string.Empty;
            }
        }
    }
}
