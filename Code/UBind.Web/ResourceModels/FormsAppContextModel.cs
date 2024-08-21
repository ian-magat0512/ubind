// <copyright file="FormsAppContextModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;

    public class FormsAppContextModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormsAppContextModel"/> class.
        /// </summary>
        /// <remarks>
        /// Changing the property requires updating `webFormApp\src\app\resource-models\organisation-summary.ts` file.
        /// </remarks>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <param name="tenantAlias">The alias of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="organisationName">The name of the organisation.</param>
        /// <param name="organisationAlias">The alias of the organisation.</param>
        /// <param name="portalId">The ID the associated portal, if any.</param>
        /// <param name="portalAlias">The alias of the portal.</param>
        /// <param name="isDefaultOrganisation">Identify whether an organisation is the default of the tenancy.</param>
        /// <param name="isMutual">Whether to use mutual wording instead of insurance wording.</param>
        public FormsAppContextModel(
            Guid tenantId,
            string tenantName,
            string tenantAlias,
            Guid organisationId,
            string organisationName,
            string organisationAlias,
            Guid productId,
            string productAlias,
            Guid? portalId,
            string? portalAlias,
            bool isDefaultOrganisation,
            bool isMutual = false)
        {
            this.TenantId = tenantId;
            this.TenantAlias = tenantAlias;
            this.TenantName = tenantName;
            this.OrganisationId = organisationId;
            this.OrganisationAlias = organisationAlias;
            this.OrganisationName = organisationName;
            this.IsDefaultOrganisation = isDefaultOrganisation;
            this.ProductId = productId;
            this.ProductAlias = productAlias;
            this.PortalId = portalId;
            this.PortalAlias = portalAlias;
            this.IsMutual = isMutual;
        }

        /// <summary>
        /// Gets the Id of the tenant.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the alias of the tenant.
        /// </summary>
        [JsonProperty]
        public string TenantAlias { get; }

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        [JsonProperty]
        public string TenantName { get; }

        /// <summary>
        /// Gets the Id of the organisation.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; }

        /// <summary>
        /// Gets the alias of the organisation.
        /// </summary>
        [JsonProperty]
        public string OrganisationAlias { get; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; }

        /// <summary>
        /// Gets a value indicating whether the given organisation is the default of the tenancy.
        /// </summary>
        [JsonProperty]
        public bool IsDefaultOrganisation { get; }

        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        [JsonProperty]
        public Guid ProductId { get; }

        /// <summary>
        /// Gets or sets the alias of the product.
        /// </summary>
        [JsonProperty]
        public string ProductAlias { get; }

        /// <summary>
        /// Gets the ID of the portal.
        /// </summary>
        [JsonProperty]
        public Guid? PortalId { get; }

        /// <summary>
        /// Gets the alias of the portal.
        /// </summary>
        [JsonProperty]
        public string? PortalAlias { get; }

        /// <summary>
        /// Gets a value indicating whether organisation is a Mutual.
        /// </summary>
        [JsonProperty]
        public bool IsMutual { get; }
    }
}
