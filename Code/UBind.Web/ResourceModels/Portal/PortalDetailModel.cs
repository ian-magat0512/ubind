// <copyright file="PortalDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Portal
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// Represents the data needed to show the details of a portal in a detail view.
    /// </summary>
    public class PortalDetailModel : PortalModel
    {
        public PortalDetailModel(PortalReadModel portal, string organisationName, string tenantName, string defaultUrl, List<AdditionalPropertyValueDto> additionalPropertyValueDtos = null)
            : base(portal, additionalPropertyValueDtos)
        {
            this.OrganisationName = organisationName;
            this.TenantName = tenantName;
            this.DefaultUrl = defaultUrl;
        }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; private set; }

        /// <summary>
        /// Gets the tenant's name.
        /// </summary>
        [JsonProperty]
        public string TenantName { get; private set; }

        public string DefaultUrl { get; private set; }
    }
}
