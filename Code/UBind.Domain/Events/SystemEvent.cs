// <copyright file="SystemEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Events
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// The system event base class that is inherited by concrete implementations.
    /// </summary>
    public class SystemEvent : Entity<Guid>
    {
        /// <summary>
        /// cached payload so we would not have to json convert the serialized payload.
        /// </summary>
        private object payload;

        [NotMapped]
        [JsonIgnore]
        private List<Relationship> relationships = null;

        [NotMapped]
        [JsonIgnore]
        private List<Tag> tags = null;

        // Parameterless constructor for EF.
        private SystemEvent()
            : base(default(Guid), default(Instant))
        {
        }

        private SystemEvent(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment deploymentEnvironment,
            SystemEventType type,
            Instant createdTimestamp,
            Instant? expiryTimestamp = null)
                : base(Guid.NewGuid(), createdTimestamp)
        {
            this.EventType = type;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = deploymentEnvironment;
            this.ExpiryTimestamp = expiryTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEvent"/> class.
        /// </summary>
        /// <param name="id">The identifier of the event.</param>
        /// <param name="payloadJson">The payload.</param>
        /// <param name="tenantId">The events string tenant id.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="productId">The events string product id.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <param name="expiryTimestamp">The expiry time stamp.</param>
        /// <param name="eventType">The system event type.</param>
        /// <param name="customEventAlias">The custom event alias.</param>
        /// <param name="relationshipJson">The serialized relationships.</param>
        [JsonConstructor]
        private SystemEvent(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            Guid id,
            string payloadJson,
            Instant createdTimestamp,
            SystemEventType eventType,
            Instant? expiryTimestamp = null,
            string customEventAlias = null,
            string relationshipJson = null)
                : base(id, createdTimestamp)
        {
            this.EventType = eventType;
            this.PayloadJson = payloadJson;
            this.RelationshipJson = relationshipJson;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = environment;
            this.CustomEventAlias = customEventAlias;
            this.ExpiryTimestamp = expiryTimestamp;
        }

        /// <summary>
        /// Gets the custom event alias.
        /// </summary>
        public string CustomEventAlias { get; private set; }

        /// <summary>
        /// Gets the system event type.
        /// </summary>
        public SystemEventType EventType { get; private set; }

        /// <summary>
        /// Gets the serialized payload in json format.
        /// </summary>
        public string PayloadJson { get; private set; }

        /// <summary>
        /// Gets the Event string product Id.
        /// </summary>
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets the Event string tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Event organisation Id.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the Event environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets or sets the time that the event will expire.
        /// </summary>
        public Instant? ExpiryTimestamp
        {
            get => this.ExpiryTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value) : (Instant?)null;

            set => this.ExpiryTicksSinceEpoch = value.HasValue ? value.Value.ToUnixTimeTicks() : null;
        }

        /// <summary>
        /// Gets the duration of the event.
        /// </summary>
        public long? ExpiryTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the serialized relationships in json format.
        /// </summary>
        public string RelationshipJson { get; private set; }

        /// <summary>
        /// Gets the serialized tags in json format.
        /// </summary>
        public string TagsJson { get; private set; }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public ReadOnlyCollection<Relationship> Relationships
        {
            get
            {
                List<Relationship> returnValue = null;

                if (this.relationships != null)
                {
                    returnValue = this.relationships;
                }
                else if (!this.RelationshipJson.IsNullOrEmpty())
                {
                    returnValue = this.relationships = JsonConvert.DeserializeObject<List<Relationship>>(this.RelationshipJson, CustomSerializerSetting.JsonSerializerSettings);
                }
                else
                {
                    returnValue = new List<Relationship>();
                }

                return returnValue.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public ReadOnlyCollection<Tag> Tags
        {
            get
            {
                List<Tag> returnValue = null;
                if (this.tags != null)
                {
                    returnValue = this.tags;
                }
                else if (!this.TagsJson.IsNullOrEmpty())
                {
                    returnValue = this.tags = JsonConvert.DeserializeObject<List<Tag>>(this.TagsJson, CustomSerializerSetting.JsonSerializerSettings);
                }
                else
                {
                    returnValue = new List<Tag>();
                }

                return returnValue.AsReadOnly();
            }
        }

        /// <summary>
        /// Create the system event object.
        /// </summary>
        /// <param name="tenantId">The events tenant ID.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="productId">The events product ID.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="eventType">The system event type.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <returns>The system event created.</returns>
        public static SystemEvent CreateWithoutPayload(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            SystemEventType eventType,
            Instant createdTimestamp) =>
            new SystemEvent(
                tenantId,
                organisationId,
                productId,
                environment,
                eventType,
                createdTimestamp,
                eventType.GetExpiryTimeStamp(createdTimestamp));

        /// <summary>
        /// Create the system event object without the product.
        /// </summary>
        /// <param name="tenantId">The events tenant ID.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="eventType">The system event type.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <returns>The system event created.</returns>
        public static SystemEvent CreateWithoutPayload(
            Guid tenantId,
            Guid organisationId,
            DeploymentEnvironment environment,
            SystemEventType eventType,
            Instant createdTimestamp) =>
            new SystemEvent(
                tenantId,
                organisationId,
                null,
                environment,
                eventType,
                createdTimestamp,
                eventType.GetExpiryTimeStamp(createdTimestamp));

        /// <summary>
        /// Create the system event object with a payload.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="tenantId">The events tenant ID.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="productId">The events product ID.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="eventType">The system event type.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <returns>The system event created.</returns>
        public static SystemEvent CreateWithPayload<TPayload>(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            SystemEventType eventType,
            TPayload payload,
            Instant createdTimestamp)
        {
            var payloadJson = JsonConvert.SerializeObject(
                payload,
                new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });

            return new SystemEvent(
                tenantId,
                organisationId,
                productId,
                environment,
                Guid.NewGuid(),
                payloadJson,
                createdTimestamp,
                eventType,
                eventType.GetExpiryTimeStamp(createdTimestamp));
        }

        /// <summary>
        /// Create the system event object with a payload without product.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="tenantId">The events tenant ID.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="eventType">The system event type.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <returns>The system event created.</returns>
        public static SystemEvent CreateWithPayload<TPayload>(
            Guid tenantId,
            Guid organisationId,
            DeploymentEnvironment environment,
            SystemEventType eventType,
            TPayload payload,
            Instant createdTimestamp)
        {
            var payloadJson = JsonConvert.SerializeObject(
                payload,
                new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                });

            return new SystemEvent(
                tenantId,
                organisationId,
                null,
                environment,
                Guid.NewGuid(),
                payloadJson,
                createdTimestamp,
                eventType,
                eventType.GetExpiryTimeStamp(createdTimestamp));
        }

        /// <summary>
        /// Create the system event object (coming from automation).
        /// </summary>
        /// <param name="tenantId">The events tenant id.</param>
        /// <param name="organisationId">The events organisation ID.</param>
        /// <param name="productId">The events product id.</param>
        /// <param name="environment">The events environment.</param>
        /// <param name="customEventAlias">The custom event alias.</param>
        /// <param name="payloadJson">The payload value.</param>
        /// <param name="createdTimestamp">The created time.</param>
        /// <param name="expiryTimestamp">The expiry time stamp.</param>
        /// <returns>The system event created.</returns>
        public static SystemEvent CreateCustom(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment,
            string customEventAlias,
            string? payloadJson,
            Instant createdTimestamp,
            Instant? expiryTimestamp = null,
            IEnumerable<Relationship> relationships = null,
            IEnumerable<Tag> tags = null)
        {
            var systemEvent = new SystemEvent(
                tenantId,
                organisationId,
                productId,
                environment,
                Guid.NewGuid(),
                payloadJson,
                createdTimestamp,
                SystemEventType.Custom,
                expiryTimestamp,
                customEventAlias);

            systemEvent.SetRelationships(relationships);
            systemEvent.SetTags(tags);

            return systemEvent;
        }

        /// <summary>
        /// Retrieves the payload.
        /// </summary>
        /// <typeparam name="TPayload">The payload type.</typeparam>
        /// <returns>The payload.</returns>
        public TPayload GetPayload<TPayload>()
            where TPayload : class
        {
            if (this.payload == null)
            {
                this.payload = JsonConvert.DeserializeObject<TPayload>(this.PayloadJson, CustomSerializerSetting.JsonSerializerSettings);
            }

            return (TPayload)this.payload;
        }

        /// <summary>
        /// Returns the data the key represents in the automation context, otherwise null.
        /// </summary>
        /// <param name="key">The map to the parameter.</param>
        /// <returns>The value the key represents.</returns>
        public JToken GetPayloadValue(string key)
        {
            JObject jObject = JObject.Parse(this.PayloadJson);
            return jObject.SelectToken(key) ?? null;
        }

        public void SetRelationships(IEnumerable<Relationship> relationships)
        {
            this.relationships = relationships != null
                ? relationships.ToList()
                : null;
            this.RelationshipJson = this.relationships != null
                ? JsonConvert.SerializeObject(this.relationships)
                : null;
        }

        public void AddRelationship(Relationship relationship)
        {
            if (this.relationships == null)
            {
                this.SetRelationships(new List<Relationship> { relationship });
            }
            else
            {
                this.relationships.Add(relationship);
                this.RelationshipJson = JsonConvert.SerializeObject(this.relationships);
            }
        }

        public void SetTags(IEnumerable<Tag> tags)
        {
            this.tags = tags != null
                ? tags.ToList()
                : null;
            this.TagsJson = this.tags != null
                ? JsonConvert.SerializeObject(this.tags)
                : null;
        }

        public void SetTags(IEnumerable<string> tags)
        {
            this.SetTags(tags.Select(t => new Tag(
                EntityType.Event, this.Id, TagType.UserDefined, t, this.CreatedTimestamp)).ToList());
        }

        public void AddTag(Tag tag)
        {
            if (this.tags == null)
            {
                this.SetTags(new List<Tag> { tag });
            }
            else
            {
                this.tags.Add(tag);
                this.TagsJson = JsonConvert.SerializeObject(this.tags);
            }
        }

        /// <summary>
        /// Create the relationships from event to the entity.
        /// </summary>
        /// <param name="type">The type of the relationship.</param>
        /// <param name="toEntitytype">The to entity type.</param>
        /// <param name="toEntityId">The to enttiy id.</param>
        public void AddRelationshipToEntity(RelationshipType type, EntityType toEntitytype, Guid? toEntityId)
        {
            if (toEntityId.HasValue && toEntityId != default)
            {
                this.AddRelationship(
                    new Relationship(
                        this.TenantId,
                        EntityType.Event,
                        this.Id,
                        type,
                        toEntitytype,
                        toEntityId.Value,
                        this.CreatedTimestamp));
            }
        }

        /// <summary>
        /// Create the relationship from entity to the event.
        /// </summary>
        /// <param name="relationshipType">The relationship type.</param>
        /// <param name="fromEntityType">The from entity type.</param>
        /// <param name="fromEntityId">The from entity id.</param>
        public void AddRelationshipFromEntity(
            RelationshipType relationshipType, EntityType fromEntityType, Guid? fromEntityId)
        {
            if (fromEntityId.HasValue && fromEntityId != default)
            {
                this.AddRelationship(
                    new Relationship(
                        this.TenantId,
                        fromEntityType,
                        fromEntityId.Value,
                        relationshipType,
                        EntityType.Event,
                        this.Id,
                        this.CreatedTimestamp));
            }
        }

        /// <summary>
        /// Removes the relationship from entity to the event.
        /// </summary>
        /// <param name="relationshipType">The relationship type.</param>
        /// <param name="fromEntityType">The from entity type.</param>
        /// <param name="fromEntityId">The from entity id.</param>
        public void RemoveRelationshipFromEntity(
            RelationshipType relationshipType, EntityType fromEntityType, Guid? fromEntityId)
        {
            if (fromEntityId.HasValue && fromEntityId != default)
            {
                var relationship = new Relationship(
                        this.TenantId,
                        fromEntityType,
                        fromEntityId.Value,
                        relationshipType,
                        EntityType.Event,
                        this.Id,
                        this.CreatedTimestamp);
                if (this.relationships != null
                    && this.relationships.Contains(relationship))
                {
                    this.relationships.Remove(relationship);
                    this.RelationshipJson = JsonConvert.SerializeObject(this.relationships);
                }
            }
        }

        /// <summary>
        /// For IAG, we need to add some hardcode to the system event.
        /// This is to make sure we have a product in the context.
        /// </summary>
        public void SetProductIdForIag(Guid? productId)
        {
            this.ProductId = productId;
        }
    }
}
