// <copyright file="Event.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of event entity
    /// that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Event : BaseEntity<SystemEvent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        public Event(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        public Event(SystemEvent model)
            : base(model.Id, model.CreatedTicksSinceEpoch, null)
        {
            this.TenantId = model.TenantId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();
            this.EventType = model.EventType.ToString().ToCamelCase();

            if (!string.IsNullOrEmpty(model.CustomEventAlias))
            {
                this.CustomEventAlias = model.CustomEventAlias.ToString();
            }

            if (model.ExpiryTicksSinceEpoch.HasValue && model.ExpiryTicksSinceEpoch != 0)
            {
                var ExpiryTimestampVariants = new DateTimeVariants(model.ExpiryTicksSinceEpoch.Value, Timezones.AET);
                this.ExpiryTicksSinceEpoch = ExpiryTimestampVariants.TicksSinceEpoch.Value;
                this.ExpiryDateTime = ExpiryTimestampVariants.DateTime;
                this.ExpiryDate = ExpiryTimestampVariants.Date;
                this.ExpiryTime = ExpiryTimestampVariants.Time;
                this.ExpiryTimeZoneName = ExpiryTimestampVariants.TimeZoneName;
                this.ExpiryTimeZoneAbbreviation = ExpiryTimestampVariants.TimeZoneAbbreviation;
            }

            if (model.Environment != DeploymentEnvironment.None)
            {
                this.EntityEnvironment = model.Environment.ToString();
            }

            if (!string.IsNullOrEmpty(model.PayloadJson))
            {
                this.EventData = JsonConvert.DeserializeObject(model.PayloadJson, CustomSerializerSetting.JsonSerializerSettings);
            }
        }

        public Event(ISystemEventWithRelatedEntities model,
            IEnumerable<string> includedProperties)
           : this(model.SystemEvent)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.SystemEvent.Tags.Any())
            {
                this.Tags = new List<Tag>();
                foreach (var tag in model.SystemEvent.Tags)
                {
                    this.Tags.Add(new Tag(tag));
                }
            }

            if (model.FromRelationships != null)
            {
                this.Relationships = model
                    .FromRelationships
                    .Select(e => new Relationship(e))
                    .Where(p => p.Name != null)
                    .ToList();
            }

            if (model.ToRelationships != null)
            {
                if (this.Relationships == null)
                {
                    this.Relationships = new List<Relationship>();
                }

                var relationships = model
                    .ToRelationships
                    .Select(e => new Relationship(e))
                    .Where(p => p.Name != null)
                    .ToList();
                this.Relationships.AddRange(relationships);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        [JsonConstructor]
        private Event()
        {
        }

        [JsonProperty(PropertyName = "expiryTicksSinceEpoch", Order = 17)]
        public long ExpiryTicksSinceEpoch { get; set; }

        [JsonProperty(PropertyName = "expiryDateTime", Order = 18)]
        public string ExpiryDateTime { get; set; }

        [JsonProperty(PropertyName = "expiryDate", Order = 19)]
        public string ExpiryDate { get; set; }

        [JsonProperty(PropertyName = "expiryTime", Order = 20)]
        public string ExpiryTime { get; set; }

        [JsonProperty(PropertyName = "expiryTimeZoneName", Order = 21)]
        public string ExpiryTimeZoneName { get; set; }

        [JsonProperty(PropertyName = "expiryTimeZoneAbbreviation", Order = 22)]
        public string ExpiryTimeZoneAbbreviation { get; set; }

        [JsonProperty(PropertyName = "tenantId", Order = 23)]
        [Required]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "tenant", Order = 24)]
        public Tenant Tenant { get; set; }

        [JsonProperty(PropertyName = "organisationId", Order = 25)]
        public string OrganisationId { get; set; }

        [JsonProperty(PropertyName = "organisation", Order = 26)]
        public Organisation Organisation { get; set; }

        [JsonProperty(PropertyName = "entityType", Order = 27)]
        public string EntityType { get; set; }

        [JsonProperty(PropertyName = "entityId", Order = 28)]
        public string EntityId { get; set; }

        [JsonProperty(PropertyName = "entity", Order = 29)]
        public object Entity { get; set; }

        [JsonProperty(PropertyName = "eventType", Order = 30)]
        [Required]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "customEventAlias", Order = 31)]
        public string CustomEventAlias { get; set; }

        [JsonProperty(PropertyName = "eventData", Order = 32)]
        [Required]
        public object EventData { get; set; }

        [JsonProperty(PropertyName = "tags", Order = 33)]
        public List<Tag> Tags { get; set; }

        [JsonProperty(PropertyName = "relationships", Order = 34)]
        public List<Relationship> Relationships { get; set; }

        public string ProductId { get; set; }

        public Product Product { get; set; }
    }
}
