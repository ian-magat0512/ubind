// <copyright file="Tenant.cs" company="uBind">
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
    /// This class is needed because we need to generate json representation of tenant entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Tenant : EntitySupportingAdditionalProperties<Domain.Tenant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Tenant(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="tenant">The tenant read model.</param>
        public Tenant(Domain.Tenant tenant)
            : base(tenant.Id, tenant.CreatedTicksSinceEpoch, tenant.Details?.CreatedTicksSinceEpoch)
        {
            if (tenant != null)
            {
                this.Alias = tenant.Details?.Alias;
                this.Name = tenant.Details?.Name ?? string.Empty;

                this.Disabled = tenant.Details?.Disabled ?? true;
                this.DefaultOrganisationId = tenant.Details.DefaultOrganisationId.ToString();
            }
        }

        public Tenant(ITenantWithRelatedEntities model, IEnumerable<string> includedProperties)
            : this(model.Tenant)
        {
            if (model.DefaultOrganisation != null)
            {
                this.DefaultOrganisation = new Organisation(model.DefaultOrganisation);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        [JsonConstructor]
        private Tenant()
        {
        }

        /// <summary>
        /// Gets or sets the alias of the tenant.
        /// </summary>
        [JsonProperty(PropertyName = "alias", Order = 3)]
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the tenant.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 4)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default organisation id for this tenant.
        /// </summary>
        [JsonProperty(PropertyName = "defaultOrganisationId", Order = 23)]
        [Required]
        public string DefaultOrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the default organisation for this tenant.
        /// </summary>
        [JsonProperty(PropertyName = "defaultOrganisation", Order = 24)]
        public Organisation DefaultOrganisation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flag if the tenant is still enable.
        /// </summary>
        [JsonProperty(PropertyName = "disabled", Order = 19)]
        [Required]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the additional properties for this tenant.
        /// </summary>
        [JsonProperty(PropertyName = "additionalProperties", Order = 25)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
