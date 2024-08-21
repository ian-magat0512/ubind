// <copyright file="EntitySupportingAdditionalProperties.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using IEntityReadModelSupportingAdditonalProperties
        = UBind.Domain.ReadModel.WithRelatedEntities.IEntitySupportingAdditionalProperties;

    public abstract class EntitySupportingAdditionalProperties<TDomainEntity> : BaseEntity<TDomainEntity>, IEntitySupportingAdditionalProperties
    {
        public EntitySupportingAdditionalProperties(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityProducedByFormsApp{TDomainEntity}"/> class.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="createdTicksSinceEpock">The created ticks since epoch.</param>
        /// <param name="lastModifiedTicksSinceEpoch">The last modified ticks since epoch.</param>
        public EntitySupportingAdditionalProperties(
            Guid id,
            long createdTicksSinceEpock,
            long? lastModifiedTicksSinceEpoch)
            : base(id, createdTicksSinceEpock, lastModifiedTicksSinceEpoch)
        {
        }

        [JsonConstructor]
        protected EntitySupportingAdditionalProperties()
        {
        }

        /// <summary>
        /// Gets or sets the additional properties of the entity, where the definition alias is the property key used.
        /// </summary>/
        public abstract Dictionary<string, object> AdditionalProperties { get; set; }

        protected void PopulateAdditionalProperties(
            IEntityReadModelSupportingAdditonalProperties entityReadModel,
            IEnumerable<string> includedProperties)
        {
            if (includedProperties?.Any(x => x.EqualsIgnoreCase(nameof(IEntitySupportingAdditionalProperties.AdditionalProperties))) == true)
            {
                this.AdditionalProperties = new Dictionary<string, object>();
                foreach (var property in entityReadModel.AdditionalPropertyValues.Where(apv => apv.Value.IsNotNullOrEmpty()).OrderBy(apv => apv.AdditionalPropertyDefinition.Alias))
                {
                    var value = property.AdditionalPropertyDefinition.PropertyType == Enums.AdditionalPropertyDefinitionType.StructuredData
                        ? JsonConvert.DeserializeObject(property.Value)
                        : property.Value;

                    if (this.AdditionalProperties.ContainsKey(property.AdditionalPropertyDefinition.Alias) == false && value != null)
                    {
                        this.AdditionalProperties.Add(property.AdditionalPropertyDefinition.Alias, value);
                    }
                }
            }
        }
    }
}
