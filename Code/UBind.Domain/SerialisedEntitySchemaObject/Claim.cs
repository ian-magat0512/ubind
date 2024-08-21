// <copyright file="Claim.cs" company="uBind">
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
    using TimeZoneNames;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// This class is needed because we need to generate json representation of claim entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Claim : EntityProducedByFormsApp<ClaimReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Claim"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Claim(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Claim"/> class.
        /// </summary>
        /// <param name="model">The claim read model with related entities.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="configProvider">The product configuration provider.</param>
        /// <param name="includedProperties">The related entities to be included.</param>
        public Claim(
            ClaimReadModel model,
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
            this.TenantId = model.TenantId.ToString();
            this.ProductId = model.ProductId.ToString();
            var ownerId = model.PersonId.ToString();
            this.OwnerId = !string.IsNullOrEmpty(ownerId) ? ownerId : null;
            this.CustomerId = model.CustomerId?.ToString();
            var policyId = model.PolicyId.ToString();
            this.PolicyId = !string.IsNullOrEmpty(policyId) ? policyId : null;
            this.Environment = model.Environment.ToString().ToCamelCase();
            this.State = model.Status?.ToCamelCase();
            this.CurrentWorkflowStep = model.WorkflowStep;
            this.ClaimReference = model.ClaimReference;
            this.ClaimNumber = !string.IsNullOrEmpty(model.ClaimNumber) ? model.ClaimNumber : null;
            this.SetFormData(model.LatestFormData);

            this.OrganisationId = model.OrganisationId.ToString();
            this.TestData = model.IsTestData;
            this.AggregateId = model.Id;

            this.EntityDescriptor = this.ClaimReference;
            this.EntityReference = this.ClaimReference;
            this.EntityEnvironment = model.Environment.ToString();

            var context = new ProductContext(model.TenantId, model.ProductId, model.Environment);
            var formDataSchema = configProvider.GetFormDataSchema(context, WebFormAppType.Claim);
            this.SetFormDataFormatted(model.LatestFormData, formDataSchema, nameof(Claim), context);

            if (includedProperties?.Any() == true)
            {
                var latestCalculation = JObject.Parse(model.SerializedLatestCalcuationResult ?? "{}");
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

            this.TimeZoneId = model.TimeZone.ToString();
            var tzNames = TZNames.GetNamesForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneName = tzNames.Generic;
            var tzAbbreviations = TZNames.GetAbbreviationsForTimeZone(model.TimeZone.Id, Locales.en_AU);
            this.TimeZoneAbbreviation = tzAbbreviations.Generic;
        }

        public Claim(
            IClaimReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver)
            : this(model.Claim, formDataPrettifier, configProvider, includedProperties)
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
                    configProvider,
                    includedProperties);
            }

            if (model.Owner != null)
            {
                this.Owner = new User(model.Owner);
            }

            if (model.Product != null)
            {
                this.Product = new SerialisedEntitySchemaObject.Product(model.Product, baseApiUrl, cachingResolver);
            }

            if (model.Tenant != null)
            {
                this.Tenant = new SerialisedEntitySchemaObject.Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.Documents != null)
            {
                this.Documents = model.Documents.Select(d => new Document(d, baseApiUrl)).ToList();
            }

            if (model.Emails != null)
            {
                this.Messages = model.Emails.Select(e => new EmailMessage(e)).ToList<Message>();
            }

            if (model.ClaimVersions != null)
            {
                this.ClaimVersions = model.ClaimVersions
                    .Select(d => new ClaimVersion(
                        d, formDataPrettifier, configProvider, includedProperties)).ToList();
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
        /// Initializes a new instance of the <see cref="Claim"/> class.
        /// </summary>
        [JsonConstructor]
        private Claim()
        {
        }

        [JsonProperty(PropertyName = "timeZoneId", Order = 2)]
        public string TimeZoneId { get; set; }

        [JsonProperty(PropertyName = "timeZoneName", Order = 3)]
        public string TimeZoneName { get; set; }

        [JsonProperty(PropertyName = "timeZoneAbbreviation", Order = 4)]
        public string TimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the claim.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 23)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the claim.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the owner id.
        /// </summary>
        [JsonProperty(PropertyName = "ownerId", Order = 25)]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the claim.
        /// </summary>
        [JsonProperty(PropertyName = "owner", Order = 26)]
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the product alias of the claim product.
        /// </summary>
        [JsonProperty(PropertyName = "productId", Order = 27)]
        [Required]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product associated with the claim.
        /// </summary>
        [JsonProperty(PropertyName = "product", Order = 28)]
        public Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer id of the customer who created the claim.
        /// </summary>
        [JsonProperty(PropertyName = "customerId", Order = 29)]
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer who created the claim.
        /// </summary>
        [JsonProperty(PropertyName = "customer", Order = 30)]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the policy id of the claim.
        /// </summary>
        [JsonProperty(PropertyName = "policyId", Order = 31)]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy created for the claim.
        /// </summary>
        [JsonProperty(PropertyName = "policy", Order = 32)]
        public Policy Policy { get; set; }

        /// <summary>
        /// Gets or sets the environment where the claim is created.
        /// </summary>
        [JsonProperty(PropertyName = "environment", Order = 33)]
        [Required]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the claim state.
        /// </summary>
        [JsonProperty(PropertyName = "state", Order = 35)]
        [Required]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the current workflow step of the claim.
        /// </summary>
        [JsonProperty(PropertyName = "currentWorkflowStep", Order = 36)]
        public string CurrentWorkflowStep { get; set; }

        /// <summary>
        /// Gets or sets the claim reference.
        /// </summary>
        [JsonProperty(PropertyName = "claimReference", Order = 37)]
        [Required]
        public string ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the claim reference.
        /// </summary>
        [JsonProperty(PropertyName = "claimNumber", Order = 38)]
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the latest calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "calculation", Order = 39)]
        public ClaimCalculation Calculation { get; set; }

        /// <summary>
        /// Gets or sets the results from the most recent calculation performed for this claim, with values that are formatted based on their data type.
        /// </summary>
        [JsonProperty(PropertyName = "calculationFormatted", Order = 40)]
        public ClaimCalculation CalculationFormatted { get; set; }

        /// <summary>
        /// Gets or sets the id of the versions l of the claim.
        /// </summary>
        [JsonProperty(PropertyName = "versionIds", Order = 41)]
        public List<string> VersionIds { get; set; }

        /// <summary>
        /// Gets or sets the versions of the claim.
        /// </summary>
        [JsonProperty(PropertyName = "claimVersions", Order = 42)]
        public List<ClaimVersion> ClaimVersions { get; set; }

        /// <summary>
        /// Gets or sets the messages associated with claim.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 46)]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 47)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the relationships associated with claim.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 48)]
        public List<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? TestData { get; set; }
    }
}
