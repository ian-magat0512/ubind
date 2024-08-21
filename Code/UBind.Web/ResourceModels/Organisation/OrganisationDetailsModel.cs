// <copyright file="OrganisationDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using Newtonsoft.Json;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Represents the details of an organisation.
    /// </summary>
    public class OrganisationDetailsModel : OrganisationModel
    {
        public OrganisationDetailsModel(
            OrganisationReadModel organisation,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos,
            string? managingOrganisationName)
            : base(organisation, additionalPropertyValueDtos)
        {
            this.MangingOrganisationId = organisation.ManagingOrganisationId;
            this.ManagingOrganisationName = managingOrganisationName;
            this.LinkedIdentities = organisation.LinkedIdentities.Select(l => new OrganisationLinkedIdentityModel(l));
        }

        /// <summary>
        /// Gets or sets the managing organisation ID.
        /// </summary>
        [JsonProperty("managingOrganisationId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? MangingOrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the managing organisation.
        /// </summary>
        [JsonProperty("managingOrganisationName", NullValueHandling = NullValueHandling.Ignore)]
        public string? ManagingOrganisationName { get; set; }

        /// <summary>
        /// Gets or sets a list of linked identities representing this organisation in an external identity provider.
        /// </summary>
        [JsonProperty("linkedIdentities", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<OrganisationLinkedIdentityModel> LinkedIdentities { get; set; }
    }
}
