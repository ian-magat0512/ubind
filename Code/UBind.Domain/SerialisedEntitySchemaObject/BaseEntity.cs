// <copyright file="BaseEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// This class is needed because we need to have a base class that will contain common properties of all serialized
    /// schema objects.
    /// </summary>
    public abstract class BaseEntity<TDomainEntity> : IEntity
    {
        protected const string DefaultState = "VIC";

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEntity{TDomainEntity}"/> class.
        /// </summary>
        public BaseEntity(Guid id)
        {
            this.Id = id;
            this.IsLoaded = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEntity{TDomainEntity}"/> class.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="createdTicksSinceEpoch">The created ticks since epoch.</param>
        /// <param name="lastModifiedTicksSinceEpoch">The last modified ticks since epoch.</param>
        public BaseEntity(Guid id, long createdTicksSinceEpoch, long? lastModifiedTicksSinceEpoch)
            : this(id)
        {
            var createdTimestampVariants = new DateTimeVariants(createdTicksSinceEpoch, Timezones.AET);
            this.CreatedTicksSinceEpoch = createdTimestampVariants.TicksSinceEpoch.Value;
            this.CreatedDateTime = createdTimestampVariants.DateTime;
            this.CreatedDate = createdTimestampVariants.Date;
            this.CreatedTime = createdTimestampVariants.Time;
            this.CreatedTimeZoneName = createdTimestampVariants.TimeZoneName;
            this.CreatedTimeZoneAbbreviation = createdTimestampVariants.TimeZoneAbbreviation;

            if (lastModifiedTicksSinceEpoch.HasValue)
            {
                var lastModifiedTimestampVariants = new DateTimeVariants(lastModifiedTicksSinceEpoch.Value, Timezones.AET);
                this.LastModifiedTicksSinceEpoch = lastModifiedTimestampVariants.TicksSinceEpoch.Value;
                this.LastModifiedDateTime = lastModifiedTimestampVariants.DateTime;
                this.LastModifiedDate = lastModifiedTimestampVariants.Date;
                this.LastModifiedTime = lastModifiedTimestampVariants.Time;
                this.LastModifiedTimeZoneName = lastModifiedTimestampVariants.TimeZoneName;
                this.LastModifiedTimeZoneAbbreviation = lastModifiedTimestampVariants.TimeZoneAbbreviation;
            }

            this.IsLoaded = true;
        }

        [JsonConstructor]
        protected BaseEntity()
        {
        }

        [JsonIgnore]
        public bool IsLoaded { get; }

        /// <summary>
        /// Gets or sets the id of the entity.
        /// </summary>
        [JsonProperty(PropertyName = "id", Order = 1)]
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the entity created ticks since epoch.
        /// </summary>
        [JsonProperty(PropertyName = "createdTicksSinceEpoch", Order = 5)]
        [Required]
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity is created.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime", Order = 6)]
        [Required]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity is created.
        /// </summary>
        [JsonProperty(PropertyName = "createdDate", Order = 7)]
        [Required]
        public string CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity is created.
        /// </summary>
        [JsonProperty(PropertyName = "createdTime", Order = 8)]
        [Required]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the create local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "createdTimeZoneName", Order = 9)]
        [Required]
        public string CreatedTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the created local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "createdTimeZoneAbbreviation", Order = 10)]
        [Required]
        public string CreatedTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the last updated ticks since epock.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTicksSinceEpoch", Order = 11)]
        public long? LastModifiedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity is last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDateTime", Order = 12)]
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the entity is entity modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedDate", Order = 13)]
        public string LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the time the entity is last modified.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTime", Order = 14)]
        public string LastModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTimeZoneName", Order = 15)]
        public string LastModifiedTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the last modififed local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "lastModifiedTimeZoneAbbreviation", Order = 16)]
        public string LastModifiedTimeZoneAbbreviation { get; set; }

        /// <summary>
        /// Gets or sets the documents in the entity.
        /// </summary>
        [JsonIgnore]
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets the domain entity type.
        /// </summary>
        [JsonIgnore]
        public Type DomainEntityType => typeof(TDomainEntity);

        /// <summary>
        /// Gets or sets the entity reference.
        /// </summary>
        [JsonIgnore]
        public string EntityReference { get; set; }

        /// <summary>
        /// Gets or sets the entity descriptor.
        /// </summary>
        [JsonIgnore]
        public string EntityDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the entity environment.
        /// </summary>
        [JsonIgnore]
        public string EntityEnvironment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity supports document.
        /// </summary>
        [JsonIgnore]
        public bool SupportsFileAttachment { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether the entity supports additional properties.
        /// </summary>
        [JsonIgnore]
        public bool SupportsAdditionalProperties
        {
            get
            {
                if (this.DomainEntityType != null)
                {
                    var result = this.DomainEntityType.InvokeMember(
                       "SupportsAdditionalProperties",
                       BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                       null,
                       null,
                       null);
                    return (bool)result;
                }

                return false;
            }
        }
    }
}
