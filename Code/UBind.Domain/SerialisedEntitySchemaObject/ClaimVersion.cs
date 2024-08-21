// <copyright file="ClaimVersion.cs" company="uBind">
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
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// This class is needed because we need to generate json representation of claim version entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class ClaimVersion : EntityProducedByFormsApp<ClaimVersionReadModel>
    {
        public ClaimVersion(Guid id)
            : base(id)
        {
        }

        public ClaimVersion(
            ClaimVersionReadModel model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties)
            : base(
                  model.Id,
                  model.CreatedTicksSinceEpoch,
                  model.LastModifiedTicksSinceEpoch,
                  formDataPrettifier,
                  includedProperties)
        {
            this.ClaimId = model.ClaimId.ToString();
            this.ClaimReference = $"{model.ClaimReferenceNumber}-{model.ClaimVersionNumber}";
            this.VersionNumber = model.ClaimVersionNumber;

            this.SetFormData(model.LatestFormData);

            var context = new ProductContext(model.TenantId, model.ProductId, model.Environment);
            var formDataSchema = configProvider.GetFormDataSchema(context, WebFormAppType.Claim);
            this.SetFormDataFormatted(model.LatestFormData, formDataSchema, nameof(ClaimVersion), context);

            this.TenantId = model.TenantId.ToString();
            this.ProductId = model.ProductId.ToString();
            this.OwnerId = model.OwnerUserId.ToString();
            this.CustomerId = model.CustomerId.ToString();
            this.PolicyId = model.PolicyId.ToString();
            this.Environment = model.Environment.ToString().ToCamelCase();

            this.OrganisationId = model.OrganisationId.ToString();
            this.IsTestData = model.IsTestData;
            this.AggregateId = model.AggregateId;

            this.EntityDescriptor = $"{this.ClaimReference}-{this.VersionNumber}";
            this.EntityReference = this.ClaimReference;
            this.EntityEnvironment = model.Environment.ToString();

            if (includedProperties?.Any() == true)
            {
                var latestCalculation = JObject.Parse(model.SerializedCalculationResult ?? "{}");
                var calculation = JObject.Parse(!string.IsNullOrWhiteSpace(latestCalculation.SelectToken("Json")?.ToString()) ? latestCalculation.SelectToken("Json").ToString() : "{}");
                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.Calculation))))
                {
                    this.Calculation = new ClaimCalculation(calculation);
                }

                if (includedProperties.Any(x => x.EqualsIgnoreCase(nameof(this.CalculationFormatted))))
                {
                    this.CalculationFormatted = new ClaimCalculation(calculation);
                }
            }
        }

        public ClaimVersion(
            IClaimVersionReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver)
            : this(
                  model.ClaimVersion,
                  formDataPrettifier,
                  configProvider,
                  includedProperties)
        {
            if (model.Claim != null)
            {
                this.Claim = new Claim(
                    model.Claim,
                    formDataPrettifier,
                    configProvider);
            }

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
        private ClaimVersion()
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

        [JsonProperty(PropertyName = "claimId", Order = 31)]
        [Required]
        public string ClaimId { get; set; }

        [JsonProperty(PropertyName = "claim", Order = 32)]
        public Claim Claim { get; set; }

        [JsonProperty(PropertyName = "policyId", Order = 33)]
        [Required]
        public string PolicyId { get; set; }

        [JsonProperty(PropertyName = "policy", Order = 34)]
        public Policy Policy { get; set; }

        [JsonProperty(PropertyName = "environment", Order = 35)]
        [Required]
        public string Environment { get; set; }

        [JsonProperty(PropertyName = "claimReference", Order = 38)]
        [Required]
        public string ClaimReference { get; set; }

        [JsonProperty(PropertyName = "versionNumber", Order = 39)]
        [Required]
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the latest calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "calculation", Order = 40)]
        public ClaimCalculation Calculation { get; set; }

        /// <summary>
        /// Gets or sets the results from the most recent calculation performed for this claim version,
        /// with values that are formatted based on their data type.
        /// </summary>
        [JsonProperty(PropertyName = "calculationFormatted", Order = 41)]
        public ClaimCalculation CalculationFormatted { get; set; }

        /// <summary>
        /// Gets or sets the emails and SMSes associated with claim version.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 49)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 50)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        [JsonProperty(PropertyName = "relationships", Order = 51)]
        public List<Relationship> Relationships { get; set; }

        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }
    }
}
