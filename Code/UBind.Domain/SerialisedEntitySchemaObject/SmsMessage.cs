// <copyright file="SmsMessage.cs" company="uBind">
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
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.ReadWriteModel;

    public class SmsMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessage"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public SmsMessage(Guid id)
            : base(id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessage"/> class.
        /// </summary>
        /// <param name="model">The message read model with related entities.</param>
        public SmsMessage(Sms model)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            this.Type = MessageType.Sms.ToString().ToCamelCase();
            this.TenantId = model.TenantId.ToString();
            this.From = model.From;
            this.To = model.To.Split(',').ToList();
            this.Content = model.Message;
            this.OrganisationId = model.OrganisationId.ToString();
        }

        public SmsMessage(ISmsReadModelWithRelatedEntities model, IEnumerable<string> includedProperties)
            : this(model.Sms)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.Tags != null)
            {
                this.Tags = model.Tags.Select(e => e.Value).ToList();
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessage"/> class.
        /// </summary>
        [JsonConstructor]
        private SmsMessage()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 30)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant associated with the message.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 31)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 32)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the message.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 33)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        [JsonProperty(PropertyName = "from", Order = 34)]
        [Required]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the to address.
        /// </summary>
        [JsonProperty(PropertyName = "to", Order = 35)]
        [Required]
        public List<string> To { get; set; }

        /// <summary>
        /// Gets or sets the body of the message in text format.
        /// </summary>
        [JsonProperty(PropertyName = "content", Order = 36)]
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the list of tags in the message.
        /// </summary>
        [JsonProperty(PropertyName = "tags", Order = 51)]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the relationships of the message.
        /// </summary>
        [JsonProperty(PropertyName = "relationships", Order = 53)]
        public List<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sms is a test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 100)]
        [DefaultValue(false)]
        public bool? TestData { get; set; }
    }
}
