// <copyright file="OrganisationReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using NodaTime;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Organisation;

    /// <summary>
    /// Read model for organisations.
    /// </summary>
    public class OrganisationReadModel : EntityReadModel<Guid>, IOrganisationReadModelSummary
    {
        /// <summary>
        /// Initializes static properties.
        /// </summary>
        static OrganisationReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        public OrganisationReadModel(
            Guid tenantId,
            Guid organisationId,
            string alias,
            string name,
            Guid? managingOrganisationId,
            bool isActive,
            bool isDeleted,
            Instant createdTimestamp)
            : base(tenantId, organisationId, createdTimestamp)
        {
            this.Alias = alias;
            this.Name = name;
            this.ManagingOrganisationId = managingOrganisationId;
            this.IsActive = isActive;
            this.IsDeleted = isDeleted;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationReadModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        public OrganisationReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the alias of the organisation.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation.
        /// </summary>
        public string Name { get; set; }

        public Guid? ManagingOrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is active or disabled.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        public bool IsDefault { get; set; }

        public Guid? DefaultPortalId { get; set; }

        /// <summary>
        /// Gets or sets the list of linked identities, which represent the organisation's record in an external
        /// identity provider.
        /// </summary>
        public virtual ICollection<OrganisationLinkedIdentityReadModel> LinkedIdentities { get; set; }
            = new Collection<OrganisationLinkedIdentityReadModel>();

        /// <summary>
        /// Gets the permissions that are excluded from all users within this organisation.
        /// TODO: Create an interface that allows people who manage orgs to choose the permissions to exclude.
        /// </summary>
        public List<Permission> ExcludedPermissions => new List<Permission>
        {
            Permission.EndorseQuotes,
            Permission.ManageTenantAdminUsers,
            Permission.AssessClaims,
            Permission.SettleClaims,
            Permission.ViewOrganisations,
            Permission.ManageOrganisations,
            Permission.ManageAllOrganisations,
            Permission.ManageProducts,
            Permission.ManageReleases,
            Permission.ViewTenants,
            Permission.ManageRoles,
            Permission.ManageRolesForAllOrganisations,
        };
    }
}
