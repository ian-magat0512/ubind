// <copyright file="Quote.cs" company="uBind">
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
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <summary>
    /// This class is needed because we need to generate json representation of quote entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Quote : EntityProducedByFormsApp<NewQuoteReadModel>
    {
        private readonly DateTimeZone timeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        /// <param name="id">The id of the quote.</param>
        public Quote(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        public Quote(
            Domain.Aggregates.Quote.Quote quote,
            IProductConfiguration productConfig,
            IFormDataPrettifier formDataPrettifier)
            : base(
                  quote.Id,
                  quote.CreatedTimestamp.ToUnixTimeTicks(),
                  quote.Aggregate.LastModifiedTicksSinceEpoch,
                  formDataPrettifier,
                  new List<string> { "calculation", "formData", "formDataFormatted" })
        {
            this.Id = quote.Id;
            this.timeZone = quote.TimeZone;
            if (quote.ExpiryTimestamp.HasValue)
            {
                var expiryDateTimeVariants = new DateTimeVariants(quote.ExpiryTimestamp.Value, this.timeZone);
                this.ExpiryTicksSinceEpoch = expiryDateTimeVariants.TicksSinceEpoch;
                this.ExpiryDateTime = expiryDateTimeVariants.DateTime;
                this.ExpiryDate = expiryDateTimeVariants.Date;
                this.ExpiryTime = expiryDateTimeVariants.Time;
                this.ExpiryTimeZoneName = expiryDateTimeVariants.TimeZoneName;
                this.ExpiryTimeZoneAbbreviation = expiryDateTimeVariants.TimeZoneAbbreviation;
            }

            this.State = quote.QuoteStatus?.ToCamelCase();
            this.QuoteReference = quote.QuoteNumber;
            var context = quote.ProductContext;
            if (quote.LatestFormData != null)
            {
                this.SetFormData(quote.LatestFormData.Data.Json);
                this.SetFormDataFormatted(quote.LatestFormData.Data.Json, productConfig.FormDataSchema, nameof(Quote), context);
            }

            this.PolicyTransactionType = quote.Type.ToString().ToCamelCase();
            this.CurrentWorkflowStep = quote.WorkflowStep;

            this.TenantId = quote.Aggregate.TenantId.ToString();
            this.OrganisationId = quote.Aggregate.OrganisationId.ToString();
            this.ProductId = quote.Aggregate.ProductId.ToString();
            this.ProductReleaseId = quote.ProductReleaseId?.ToString();

            this.Environment = quote.Aggregate.Environment.ToString().ToCamelCase();
            this.CustomerId = quote.CustomerId;
            if (quote.Aggregate.Policy != null)
            {
                this.PolicyId = quote.Aggregate.Policy.PolicyId.ToString();
                var lastTransactionId = quote.Aggregate?.Policy?.Transactions?.LastOrDefault()?.Id;
                if (lastTransactionId != null)
                {
                    this.PolicyTransactionId = lastTransactionId.ToString();
                }
            }

            this.OwnerId = quote.Aggregate.OwnerUserId.ToString();

            this.InvoiceNumber = quote.InvoiceNumber;
            this.CreditNoteNumber = quote.CreditNoteNumber;

            this.TestData = quote.IsTestData;
            this.AggregateId = quote.Aggregate.Id;
            this.EntityReference = this.EntityDescriptor = this.QuoteReference;
            this.EntityEnvironment = quote.Aggregate.Environment.ToString();
            this.QuoteVersions = new List<QuoteVersion>();
            this.Messages = new List<Message>();
            this.Relationships = new List<Relationship>();
            this.Documents = new List<Document>();
            if (quote.LatestCalculationResult != null)
            {
                this.Calculation = new QuoteCalculation(quote.LatestCalculationResult.Data);
                this.CalculationFormatted = new QuoteCalculation(quote.LatestCalculationResult.Data, true);
            }

            this.TimeZoneId = quote.TimeZone.ToString();
            var tzNames = TZNames.GetNamesForTimeZone(quote.TimeZone.Id, Locales.en_AU);
            this.TimeZoneName = tzNames.Generic;
            var tzAbbreviations = TZNames.GetAbbreviationsForTimeZone(quote.TimeZone.Id, Locales.en_AU);
            this.TimeZoneAbbreviation = tzAbbreviations.Generic;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        /// <param name="model">The quote read model with related entities.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="configProvider">The product configuration provider.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        public Quote(
            NewQuoteReadModel model,
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

            if (model.ExpiryTicksSinceEpoch.HasValue)
            {
                var expiryDateTimeVariants = new DateTimeVariants(model.ExpiryTicksSinceEpoch.Value, this.timeZone);
                this.ExpiryTicksSinceEpoch = expiryDateTimeVariants.TicksSinceEpoch;
                this.ExpiryDateTime = expiryDateTimeVariants.DateTime;
                this.ExpiryDate = expiryDateTimeVariants.Date;
                this.ExpiryTime = expiryDateTimeVariants.Time;
                this.ExpiryTimeZoneName = expiryDateTimeVariants.TimeZoneName;
                this.ExpiryTimeZoneAbbreviation = expiryDateTimeVariants.TimeZoneAbbreviation;
            }

            this.State = model.QuoteState?.ToCamelCase();
            this.QuoteReference = model.QuoteNumber;
            this.SetFormData(model.LatestFormData);

            FormDataSchema? formDataSchema;
            var productContext = new ProductContext(
                model.TenantId,
                model.ProductId,
                model.Environment);
            if (model.ProductReleaseId != null)
            {
                var context = new ReleaseContext(
                    model.TenantId,
                    model.ProductId,
                    model.Environment,
                    model.ProductReleaseId.Value);
                formDataSchema = configProvider.GetFormDataSchema(context, WebFormAppType.Quote);
            }
            else
            {
                formDataSchema = configProvider.GetFormDataSchema(productContext, WebFormAppType.Quote);
            }

            this.SetFormDataFormatted(model.LatestFormData, formDataSchema, nameof(Quote), productContext);

            this.PolicyTransactionType = model.Type.ToString().ToCamelCase();
            this.CurrentWorkflowStep = model.WorkflowStep;

            this.TenantId = model.TenantId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();
            this.ProductId = model.ProductId.ToString();
            this.ProductReleaseId = model.ProductReleaseId?.ToString();

            this.Environment = model.Environment.ToString().ToCamelCase();
            this.CustomerId = model.CustomerId;
            if (model.PolicyId.HasValue)
            {
                this.PolicyId = model.PolicyId.ToString();
            }

            this.OwnerId = model.OwnerUserId.HasValue ? model.OwnerUserId.ToString() : null;
            if (model.PolicyTransactionId.HasValue)
            {
                this.PolicyTransactionId = model.PolicyTransactionId.ToString();
            }

            this.InvoiceNumber = model.InvoiceNumber;
            this.CreditNoteNumber = model.CreditNoteNumber;

            if (model.PaymentResponseJson.IsNotNullOrEmpty())
            {
                var deftResponse = JObject.Parse(model.PaymentResponseJson);
                this.DeftPaymentReferenceNumber = deftResponse.ContainsKey("Drn") ? deftResponse["Drn"].ToString() : null;
                this.DeftCustomerReferenceNumber = deftResponse.ContainsKey("Crn") ? deftResponse["Crn"].ToString() : null;
            }

            this.TestData = model.IsTestData;
            this.AggregateId = model.AggregateId;
            this.EntityDescriptor = this.QuoteReference;
            this.EntityReference = this.QuoteReference;
            this.EntityEnvironment = model.Environment.ToString();

            if (includedProperties?.Any() == true)
            {
                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.Calculation))))
                {
                    this.Calculation = new QuoteCalculation(model.SerializedLatestCalculationResult);
                }

                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.CalculationFormatted))))
                {
                    this.CalculationFormatted = new QuoteCalculation(model.SerializedLatestCalculationResult, true);
                }
            }

            this.TimeZoneId = model.TimeZone.ToString();
            var tzNames = TZNames.GetNamesForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneName = tzNames.Generic;
            var tzAbbreviations = TZNames.GetAbbreviationsForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneAbbreviation = tzAbbreviations.Generic;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        /// <param name="model">The quote read model with related entities.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="configProvider">The product configuration provider.</param>
        public Quote(
            IQuoteReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        : this(model.Quote, formDataPrettifier, configProvider, includedProperties)
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

            if (model.PolicyTransaction != null)
            {
                this.PolicyTransaction = new PolicyTransaction(
                    model.PolicyTransaction,
                    formDataPrettifier,
                    configProvider,
                    Enumerable.Empty<string>(),
                    model.Quote.TimeZone,
                    timeOfDayScheme);
            }

            if (model.Owner != null)
            {
                this.Owner = new User(model.Owner);
            }

            if (model.Product != null)
            {
                this.Product = new Product(model.Product, baseApiUrl, cachingResolver);
            }

            if (model.ProductRelease != null)
            {
                this.ProductRelease = new ProductRelease(model.ProductRelease);
            }

            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.Documents != null)
            {
                this.Documents = model.Documents.Select(e => new Document(e, baseApiUrl)).ToList();
            }

            if (model.Emails != null)
            {
                this.Messages = model.Emails.Select(e => new EmailMessage(e)).ToList<Message>();
            }

            if (model.QuoteVersions != null)
            {
                this.QuoteVersions = model.QuoteVersions
                    .Select(e => new QuoteVersion(e, formDataPrettifier, configProvider))
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
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        [JsonConstructor]
        private Quote()
        {
        }

        [JsonProperty(PropertyName = "timeZoneId", Order = 2)]
        public string TimeZoneId { get; set; }

        [JsonProperty(PropertyName = "timeZoneName", Order = 3)]
        public string TimeZoneName { get; set; }

        [JsonProperty(PropertyName = "timeZoneAbbreviation", Order = 4)]
        public string TimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the expiry ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "expiryTicksSinceEpoch", Order = 21)]
        public long? ExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDateTime", Order = 22)]
        public string ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryDate", Order = 23)]
        public string ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity will be expired.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTime", Order = 24)]
        public string ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZoneName", Order = 25)]
        public string ExpiryTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the expiry local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "expiryTimeZoneAbbreviation", Order = 26)]
        public string ExpiryTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 31)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the quote.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 32)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 33)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the quote.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 34)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the owner id.
        /// </summary>
        [JsonProperty(PropertyName = "ownerId", Order = 35)]
        public string? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "owner", Order = 36)]
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the product alias of the quote product.
        /// </summary>
        [JsonProperty(PropertyName = "productId", Order = 37)]
        [Required]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product associated with the quote.
        /// </summary>
        [JsonProperty(PropertyName = "product", Order = 38)]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "productReleaseId", Order = 39)]
        public string ProductReleaseId { get; set; }

        [JsonProperty(PropertyName = "productRelease", Order = 40)]
        public ProductRelease ProductRelease { get; set; }

        /// <summary>
        /// Gets or sets the customer id of the customer who created the quote.
        /// </summary>
        [JsonProperty(PropertyName = "customerId", Order = 41)]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer who created the quote.
        /// </summary>
        [JsonProperty(PropertyName = "customer", Order = 42)]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the policy id of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "policyId", Order = 43)]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy created for the quote.
        /// </summary>
        [JsonProperty(PropertyName = "policy", Order = 44)]
        public Policy Policy { get; set; }

        /// <summary>
        /// Gets or sets the environment where the quote is created.
        /// </summary>
        [JsonProperty(PropertyName = "environment", Order = 45)]
        [Required]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction type of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactionType", Order = 46)]
        [Required]
        public string PolicyTransactionType { get; set; }

        /// <summary>
        /// Gets or sets the quote state.
        /// </summary>
        [JsonProperty(PropertyName = "state", Order = 47)]
        [Required]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the current workflow step of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "currentWorkflowStep", Order = 48)]
        public string CurrentWorkflowStep { get; set; }

        /// <summary>
        /// Gets or sets the quote reference.
        /// </summary>
        [JsonProperty(PropertyName = "quoteReference", Order = 49)]
        public string QuoteReference { get; set; }

        /// <summary>
        /// Gets or sets the latest calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "calculation", Order = 50)]
        public QuoteCalculation Calculation { get; set; }

        /// <summary>
        /// Gets or sets the results from the most recent calculation performed for this quote, with values that are formatted based on their data type.
        /// </summary>
        [JsonProperty(PropertyName = "calculationFormatted", Order = 51)]
        public QuoteCalculation CalculationFormatted { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction Id of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactionId", Order = 52)]
        public string PolicyTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransaction", Order = 53)]
        public PolicyTransaction PolicyTransaction { get; set; }

        /// <summary>
        /// Gets or sets the invoice id of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "invoiceId", Order = 54)]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the credit note Id of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "creditNoteId", Order = 55)]
        public string CreditNoteId { get; set; }

        /// <summary>
        /// Gets or sets the id of the versions l of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "versionIds", Order = 56)]
        public List<string> VersionIds { get; set; }

        /// <summary>
        /// Gets or sets the versions of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "quoteVersions", Order = 57)]
        public List<QuoteVersion> QuoteVersions { get; set; }

        /// <summary>
        /// Gets or sets the sms generated in the quote.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 59)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 60)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the sms generated in the quote.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 61)]
        public List<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets the invoice number for this quote.
        /// </summary>
        [JsonProperty(PropertyName = "invoiceNumber", Order = 62)]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets the credit note number for this quote, if any.
        /// </summary>
        [JsonProperty(PropertyName = "creditNoteNumber", Order = 63)]
        public string CreditNoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the payment response DRN received when payment was made via DEFT, if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "deftPaymentReferenceNumber", Order = 64)]
        public string DeftPaymentReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the payment response CRN received when payment was made via DEFT, if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "deftCustomerReferenceNumber", Order = 65)]
        public string DeftCustomerReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? TestData { get; set; }
    }
}
