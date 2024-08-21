// <copyright file="Organisation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of product entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Organisation : EntitySupportingAdditionalProperties<OrganisationReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Organisation"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Organisation(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Organisation"/> class.
        /// </summary>
        /// <param name="model">The organisation read model with related entities.</param>
        public Organisation(OrganisationReadModel model)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.Alias = model.Alias;
            this.Name = model.Name;

            this.TenantId = model.TenantId.ToString();
            this.Disabled = !model.IsActive;

            this.EntityDescriptor = this.Name;
            this.EntityReference = this.Name;
        }

        public Organisation(IOrganisationReadModelWithRelatedEntities model, IEnumerable<string> includedProperties)
            : this(model.Organisation)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Organisation"/> class.
        /// </summary>
        [JsonConstructor]
        private Organisation()
        {
        }

        /// <summary>
        /// Gets or sets the alias of the organisation.
        /// </summary>
        [JsonProperty(PropertyName = "alias", Order = 2)]
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 3)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of the organisation.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the organisation.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flag if the organisation is still enable.
        /// </summary>
        [JsonProperty(PropertyName = "disabled", Order = 23)]
        [Required]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the additional properties for this organisation.
        /// </summary>
        [JsonProperty(PropertyName = "additionalProperties", Order = 24)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
