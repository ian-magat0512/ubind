// <copyright file="User.cs" company="uBind">
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
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// This class is needed because we need to generate json representation of user entity
    /// that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class User : EntitySupportingAdditionalProperties<UserReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">The id of the user.</param>
        public User(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="model">The user read model with related entities.</param>
        public User(UserReadModel model)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.PersonId = model.PersonId.ToString();
            this.UserType = model.UserType != null ? model.UserType.ToCamelCase() : string.Empty;
            this.TenantId = model.TenantId.ToString();
            this.OrganisationId = model.OrganisationId.ToString();

            this.Permissions = model.Roles.SelectMany(c => c.Permissions)
                .Distinct().Select(c => new Permission(c.ToString())).ToList();

            this.PortalId = model.PortalId.HasValue ? model.PortalId.Value.ToString() : null;
            this.Disabled = model.IsDisabled;
            this.AccountEmailAddress = model.LoginEmail;

            if (model.Roles.Any())
            {
                this.Roles = model.Roles.Select(role => new Role(role)).ToList();
            }

            if (this.Person != null)
            {
                this.EntityDescriptor = $"{this.Person.FullName} ({this.Person.EmailAddresses[0].Email})";
                this.EntityReference = this.Person.EmailAddresses[0].Email;
            }
        }

        public User(IUserReadModelWithRelatedEntities model, IEnumerable<string> includedProperties)
            : this(model.User)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            if (model.Person != null)
            {
                this.Person = new Person(model.Person);
            }

            if (model.Roles != null)
            {
                this.Roles = model.Roles.Select(e => new Role(e)).ToList();
            }

            if (model.Portal != null)
            {
                this.Portal = new Portal(model.Portal, model.PortalLocations);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        [JsonConstructor]
        private User()
        {
        }

        /// <summary>
        /// Gets or sets the tenant id of the user.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the user.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 23)]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 24)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "personId", Order = 25)]
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the user.
        /// </summary>
        [JsonProperty(PropertyName = "person", Order = 26)]
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets the role ids of the user.
        /// </summary>
        [JsonProperty(PropertyName = "roleIds", Order = 28)]
        public IEnumerable<string> RoleIds { get; set; }

        /// <summary>
        /// Gets or sets the role of the user.
        /// </summary>
        [JsonProperty(PropertyName = "roles", Order = 29)]
        public List<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the permissions of the user.
        /// </summary>
        [JsonProperty(PropertyName = "permissions", Order = 30)]
        public IEnumerable<Permission> Permissions { get; set; }

        /// <summary>
        /// Gets or sets the user type.
        /// </summary>
        [JsonProperty(PropertyName = "type", Order = 31)]
        public string UserType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flag if the user is still enable.
        /// </summary>
        [JsonProperty(PropertyName = "disabled", Order = 32)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal associated with the user, if any.
        /// </summary>
        [JsonProperty(PropertyName = "portalId", Order = 33)]
        public string PortalId { get; set; }

        /// <summary>
        /// Gets or sets the portal associated with the user.
        /// </summary>
        [JsonProperty(PropertyName = "portal", Order = 34)]
        public Portal Portal { get; set; }

        /// <summary>
        /// Gets or sets the account email address.
        /// </summary>
        [JsonProperty(PropertyName = "accountEmailAddress", Order = 35)]
        public string AccountEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the additional properties associated with the user.
        /// </summary>
        [JsonProperty(PropertyName = "additionalProperties", Order = 40)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
