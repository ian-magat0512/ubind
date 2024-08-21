// <copyright file="Role.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed because we need to generate json representation of role entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Role : BaseEntity<Domain.Entities.Role>
    {
        public Role(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Role"/> class.
        /// </summary>
        /// <param name="model">The role read model.</param>
        public Role(UBind.Domain.Entities.Role model)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.TenantId = model.TenantId.ToString();
            this.Name = model.Name;
            this.Description = model.Description;
            this.Permissions = model.Permissions != null ?
                model.Permissions.Select(c => new Permission(c.ToString())).ToList() : null;
        }

        /// <summary>
        /// Gets or sets the alias of the role.
        /// </summary>
        [JsonProperty(PropertyName = "alias", Order = 2)]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        [JsonProperty(PropertyName = "description", Order = 4)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the role.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 12)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 13)]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the role.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 14)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of the role.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 17)]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the permissions of the user.
        /// </summary>
        [JsonProperty(PropertyName = "permissions", Order = 18)]
        public IEnumerable<Permission> Permissions { get; set; }
    }
}
