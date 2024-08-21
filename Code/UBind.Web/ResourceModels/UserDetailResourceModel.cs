// <copyright file="UserDetailResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Web.ResourceModels.User;

    public class UserDetailResourceModel : UserResourceModel
    {
        public UserDetailResourceModel(
            UserReadModelDetail user,
            PortalReadModel? portal,
            IPersonReadModelSummary person = null,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos = null)
            : base(user, additionalPropertyValueDtos, person)
        {
            this.OrganisationName = user.OrganisationName;
            this.OrganisationAlias = user.OrganisationAlias;
            this.PortalId = portal?.Id;
            this.PortalName = portal?.Name;
            this.LinkedIdentities = user.LinkedIdentities.Select(l => new UserLinkedIdentityModel(l));
        }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        public string OrganisationName { get; private set; }

        /// <summary>
        /// Gets the portal ID.
        /// </summary>
        public Guid? PortalId { get; private set; }

        /// <summary>
        /// Gets the portal name.
        /// </summary>
        public string PortalName { get; private set; }

        /// <summary>
        /// Gets or sets a list of linked identities representing this organisation in an external identity provider.
        /// </summary>
        [JsonProperty("linkedIdentities", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<UserLinkedIdentityModel> LinkedIdentities { get; set; }
    }
}
