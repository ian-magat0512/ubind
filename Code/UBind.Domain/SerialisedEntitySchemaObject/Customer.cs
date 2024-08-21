// <copyright file="Customer.cs" company="uBind">
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
    using MoreLinq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// This class is needed because we need to generate json representation of customer entity that conforms with
    /// serialized-entity-schema.json file.
    /// </summary>
    public class Customer : EntitySupportingAdditionalProperties<CustomerReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Customer(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        /// <param name="model">The customer read model with related entities.</param>
        public Customer(CustomerReadModel model)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.TenantId = model.TenantId.ToString();
            this.OwnerId = model.OwnerUserId.ToString();
            this.Environment = model.Environment.ToString().ToCamelCase();
            this.PrimaryPersonId = model.PrimaryPersonId.ToString();
            this.IsTestData = model.IsTestData;
            this.OrganisationId = model.OrganisationId.ToString();
            this.PortalId = model.PortalId.HasValue ? model.PortalId.Value.ToString() : null;
            this.EntityEnvironment = model.Environment.ToString();
            if (model.PrimaryPerson != null)
            {
                this.EntityDescriptor = $"{model.PrimaryPerson.DisplayName} ({model.PrimaryPerson.Email})";
                this.EntityReference = model.PrimaryPerson.Email;
            }
        }

        public Customer(
            ICustomerReadModelWithRelatedEntities model,
            IFormDataPrettifier formDataPrettifier,
            IProductConfigurationProvider configProvider,
            IEnumerable<string> includedProperties,
            string baseApiUrl)
            : this(model.Customer)
        {
            if (model.Owner != null)
            {
                this.Owner = new User(model.Owner);
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

            if (model.Policies != null)
            {
                this.Policies = model.Policies
                    .Select(e =>
                    new Policy(
                        e,
                        model.PolicyTransactions.Where(
                        pt => pt.PolicyId == e.Id),
                        formDataPrettifier,
                        configProvider))
                    .ToList();
            }

            if (model.Claims != null)
            {
                this.Claims = model.Claims
                    .Select(e => new Claim(e, formDataPrettifier, configProvider))
                    .ToList();
            }

            if (model.Quotes != null)
            {
                this.Quotes = model.Quotes
                    .Select(e => new Quote(e, formDataPrettifier, configProvider))
                    .ToList();
            }

            if (model.PrimaryPerson != null)
            {
                this.PrimaryPerson = new Person(model.PrimaryPerson);
            }

            if (model.People != null)
            {
                this.People = model.People.Select(e => new Person(e)).ToList();
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

            if (model.Portal != null)
            {
                this.Portal = new Portal(model.Portal, model.PortalLocations);
            }

            if (model.SavedPaymentMethods != null)
            {
                model.SavedPaymentMethods.ForEach(x => x.ClearSensitiveInformation());
                this.SavedPaymentMethods = model.SavedPaymentMethods.Select(s => new CustomerPaymentMethod(s)).ToList();
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        [JsonConstructor]
        private Customer()
        {
        }

        /// <summary>
        /// Gets or sets the tenant id of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the customer.
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
        /// Gets or sets the organization associated with the customer.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the owner id.
        /// </summary>
        [JsonProperty(PropertyName = "ownerId", Order = 25)]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the owner of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "owner", Order = 26)]
        public User Owner { get; set; }

        /// <summary>
        /// Gets or sets the environment of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "environment", Order = 27)]
        [Required]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the primary person Id.
        /// </summary>
        [JsonProperty(PropertyName = "primaryPersonId", Order = 27)]
        [Required]
        public string PrimaryPersonId { get; set; }

        /// <summary>
        /// Gets or sets the primary person.
        /// </summary>
        [JsonProperty(PropertyName = "primaryPerson", Order = 27)]
        public Person PrimaryPerson { get; set; }

        /// <summary>
        /// Gets or sets the IDs of the people associated with the customer.
        /// </summary>
        [JsonProperty(PropertyName = "personIds", Order = 28)]
        public List<string> PersonIds { get; set; }

        /// <summary>
        /// Gets or sets the list of people associated with the customer.
        /// </summary>
        [JsonProperty(PropertyName = "people", Order = 29)]
        public List<Person> People { get; set; }

        /// <summary>
        /// Gets or sets the quote ids of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "quoteIds", Order = 30)]
        public List<string> QuoteIds { get; set; }

        /// <summary>
        /// Gets or sets the quotes of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "quotes", Order = 31)]
        public List<Quote> Quotes { get; set; }

        /// <summary>
        /// Gets or sets the policy ids of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "policyIds", Order = 32)]
        public List<string> PolicyIds { get; set; }

        /// <summary>
        /// Gets or sets the policies of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "policies", Order = 33)]
        public List<Policy> Policies { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction ids of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactionIds", Order = 34)]
        public List<string> PolicyTransactionIds { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "policyTransactions", Order = 35)]
        public List<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the IDs of the claims associated with the customer.
        /// </summary>
        [JsonProperty(PropertyName = "claimIds", Order = 36)]
        public List<string> ClaimIds { get; set; }

        /// <summary>
        /// Gets or sets the claims of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "claims", Order = 37)]
        public List<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets the document ids of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "documentIds", Order = 40)]
        public List<string> DocumentIds { get; set; }

        /// <summary>
        /// Gets or sets the documents of the customer.
        /// </summary>
        [JsonProperty(PropertyName = "documents", Order = 41)]
        public List<Document> Documents { get; set; }

        /// <summary>
        /// Gets or sets the messages generated by the customer.
        /// </summary>
        [JsonProperty(PropertyName = "messages", Order = 42)]
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the portal ID related to the customer.
        /// </summary>
        [JsonProperty(PropertyName = "portalId", Order = 42)]
        public string PortalId { get; set; }

        /// <summary>
        /// Gets or sets the portal the customer is associated with.
        /// </summary>
        [JsonProperty(PropertyName = "portal", Order = 43)]
        public Portal Portal { get; set; }

        /// <summary>
        /// Gets or sets the additional properties associated with the customer.
        /// </summary>
        [JsonProperty(PropertyName = "additionalProperties", Order = 44)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the messages generated by the customer.
        /// </summary>
        [JsonProperty(PropertyName = "customerPaymentMethods", Order = 45)]
        public List<CustomerPaymentMethod> SavedPaymentMethods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customer is a test data only.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 46)]
        [DefaultValue(false)]
        public bool? IsTestData { get; set; }
    }
}
