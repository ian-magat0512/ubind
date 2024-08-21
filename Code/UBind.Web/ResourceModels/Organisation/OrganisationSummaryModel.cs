// <copyright file="OrganisationSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using System;
    using Newtonsoft.Json;

    [Obsolete("This has been replacted by FormsAppContextModel and PortalAppContextModel. To be removed in UB-9510")]
    public class OrganisationSummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationSummaryModel"/> class.
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
        /// <param name="portalTitle">The title of the portal.</param>
        /// <param name="isDefaultOrganisation">Identify whether an organisation is the default of the tenancy.</param>
        /// <param name="portalStylesheetUrl">The stylesheet url of the portal.</param>
        /// <param name="customDomain">The custom domain.</param>
        public OrganisationSummaryModel(
            Guid tenantId,
            string tenantName,
            string tenantAlias,
            Guid organisationId,
            string organisationName,
            string organisationAlias,
            Guid? portalId,
            string portalAlias,
            string portalTitle,
            bool isDefaultOrganisation,
            string portalStylesheetUrl,
            string customDomain)
        {
            this.OrganisationId = organisationId;
            this.OrganisationName = organisationName;
            this.OrganisationAlias = organisationAlias;
            this.IsDefaultOrganisation = isDefaultOrganisation;

            this.TenantId = tenantId;
            this.TenantName = tenantName;
            this.TenantAlias = tenantAlias;
            this.PortalId = portalId;
            this.PortalAlias = portalAlias;
            this.PortalTitle = portalTitle;
            this.PortalStylesheetUrl = portalStylesheetUrl;
            this.IsMutual = this.IsMutualOrganisation();
            this.CustomDomain = customDomain;
        }

        /// <summary>
        /// Gets the Id of the tenant.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        [JsonProperty]
        public string TenantName { get; }

        /// <summary>
        /// Gets the alias of the tenant.
        /// </summary>
        [JsonProperty]
        public string TenantAlias { get; }

        /// <summary>
        /// Gets the custom domain.
        /// </summary>
        [JsonProperty]
        public string CustomDomain { get; }

        /// <summary>
        /// Gets a value indicating whether organisation is a Mutual.
        /// </summary>
        [JsonProperty]
        public bool IsMutual { get; }

        /// <summary>
        /// Gets the Id of the organisation.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; }

        /// <summary>
        /// Gets the alias of the organisation.
        /// </summary>
        [JsonProperty]
        public string OrganisationAlias { get; }

        /// <summary>
        /// Gets the ID of the portal.
        /// </summary>
        [JsonProperty]
        public Guid? PortalId { get; }

        /// <summary>
        /// Gets the title of the portal.
        /// </summary>
        [JsonProperty]
        public string PortalTitle { get; }

        /// <summary>
        /// Gets the alias of the portal.
        /// </summary>
        [JsonProperty]
        public string PortalAlias { get; }

        /// <summary>
        /// Gets a value indicating whether the given organisation is the default of the tenancy.
        /// </summary>
        [JsonProperty]
        public bool IsDefaultOrganisation { get; }

        /// <summary>
        /// Gets the stylesheet url of the portal.
        /// </summary>
        [JsonProperty]
        public string PortalStylesheetUrl { get; }

        private bool IsMutualOrganisation()
        {
            return false;
        }
    }
}
