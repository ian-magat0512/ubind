// <copyright file="Portal.cs" company="uBind">
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
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// This class is needed because we need to generate json representation of portal entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Portal : EntitySupportingAdditionalProperties<PortalReadModel>
    {
        public Portal(Guid id)
            : base(id)
        {
        }

        public Portal(
            PortalReadModel portal,
            PortalLocations locations)
            : base(portal.Id, portal.CreatedTicksSinceEpoch, portal.CreatedTicksSinceEpoch)
        {
            this.TenantId = portal.TenantId.ToString();
            this.OrganisationId = portal.OrganisationId.ToString();
            this.Alias = portal.Alias;
            this.Name = portal.Name;
            this.Title = portal.Title;
            this.Disabled = portal.Disabled;
            this.Locations = locations;
            this.EntityDescriptor = this.Name;
            this.EntityReference = this.Name;
            this.OrganisationId = portal.OrganisationId.ToString();
        }

        public Portal(
            IPortalWithRelatedEntities model,
            IEnumerable<string> includedProperties,
            PortalLocations locations)
            : this(model.Portal, locations)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new SerialisedEntitySchemaObject.Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new SerialisedEntitySchemaObject.Organisation(model.Organisation);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        [JsonConstructor]
        private Portal()
        {
        }

        [JsonProperty(PropertyName = "alias", Order = 2)]
        [Required]
        public string Alias { get; set; }

        [JsonProperty(PropertyName = "name", Order = 3)]
        [Required]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "title", Order = 4)]
        [Required]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        [JsonProperty(PropertyName = "organisationId", Order = 23)]
        public string OrganisationId { get; set; }

        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", Order = 25)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flag if the portal is still enable.
        /// </summary>
        [JsonProperty(PropertyName = "disabled", Order = 26)]
        [Required]
        public bool Disabled { get; set; }

        [JsonProperty(PropertyName = "locations", Order = 27)]
        public PortalLocations Locations { get; set; }
    }
}
