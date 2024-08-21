// <copyright file="QuoteVersion.cs" company="uBind">
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
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of quote version entity that conforms
    /// with serialized-entity-schema.json file.
    /// </summary>
    public class QuoteVersion : EntityProducedByFormsApp<QuoteVersionReadModel>
    {
        public QuoteVersion(Guid id)
            : base(id)
        {
        }

        public QuoteVersion(
            QuoteVersionReadModel model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties = default)
            : base(
                  model.QuoteVersionId,
                  model.CreatedTicksSinceEpoch,
                  model.LastModifiedTicksSinceEpoch,
                  formDataPrettifier,
                  includedProperties)
        {
            this.QuoteId = model.QuoteId.ToString();
            this.QuoteReference = $"{model.QuoteNumber}-{model.QuoteVersionNumber}";
            this.VersionNumber = model.QuoteVersionNumber;
            this.SetFormData(model.LatestFormData);

            var productContext = new ProductContext(model.TenantId, model.ProductId, model.Environment);
            var formDataSchema = configProvider.GetFormDataSchema(productContext, WebFormAppType.Quote);
            this.SetFormDataFormatted(model.LatestFormData, formDataSchema, nameof(QuoteVersion), productContext);

            this.CustomerId = model.CustomerId.ToString();
            this.OwnerId = model.OwnerUserId.ToString();
            this.TenantId = model.TenantId.ToString();
            this.ProductId = model.ProductId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();
            this.PolicyTransactionType = model.Type.ToString().ToCamelCase();
            this.Environment = model.Environment.ToString().ToCamelCase();
            this.IsTestData = model.IsTestData;
            this.AggregateId = model.AggregateId;
            if (includedProperties?.Any() == true)
            {
                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.Calculation))))
                {
                    this.Calculation = new QuoteCalculation(model.SerializedCalculationResult);
                }

                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.CalculationFormatted))))
                {
                    this.CalculationFormatted = new QuoteCalculation(model.SerializedCalculationResult, true);
                }
            }

            this.EntityDescriptor = $"{this.QuoteReference}-{this.VersionNumber}";
            this.EntityReference = this.QuoteReference;
            this.EntityEnvironment = model.Environment.ToString();
        }

        public QuoteVersion(
            IQuoteVersionReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver)
            : this(model.QuoteVersion, formDataPrettifier, configProvider, includedProperties)
        {
            if (model.Owner != null)
            {
                this.Owner = new User(model.Owner);
            }

            if (model.Customer != null)
            {
                this.Customer = new Customer(model.Customer);
            }

            if (model.Product != null)
            {
                this.Product = new Product(
                    model.Product, baseApiUrl, cachingResolver);
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

        [JsonConstructor]
        private QuoteVersion()
        {
        }

        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        [JsonProperty(PropertyName = "organisationId", Order = 23)]
        [Required]
        public string OrganisationId { get; set; }

        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        [JsonProperty(PropertyName = "ownerId", Order = 25)]
        public string OwnerId { get; set; }

        [JsonProperty(PropertyName = "owner", Order = 26)]
        public User Owner { get; set; }

        [JsonProperty(PropertyName = "productId", Order = 27)]
        [Required]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "product", Order = 28)]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "customerId", Order = 29)]
        [Required]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "customer", Order = 30)]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "quoteId", Order = 31)]
        [Required]
        public string QuoteId { get; set; }

        [JsonProperty(PropertyName = "quote", Order = 32)]
        public Quote Quote { get; set; }

        [JsonProperty(PropertyName = "environment", Order = 33)]
        [Required]
        public string Environment { get; set; }

        [JsonProperty(PropertyName = "policyTransactionType", Order = 24)]
        [Required]
        public string PolicyTransactionType { get; set; }

        [JsonProperty(PropertyName = "quoteReference", Order = 35)]
        [Required]
        public string QuoteReference { get; set; }

        [JsonProperty(PropertyName = "calculation", Order = 36)]
        public QuoteCalculation Calculation { get; set; }

        [JsonProperty(PropertyName = "calculationFormatted", Order = 37)]
        public QuoteCalculation CalculationFormatted { get; set; }

        [JsonProperty(PropertyName = "versionNumber", Order = 38)]
        [Required]
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the emails and SMSes generated in the quote.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 42)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 43)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        [JsonProperty(PropertyName = "relationships", Order = 44)]
        public List<Relationship> Relationships { get; set; }

        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }
    }
}
