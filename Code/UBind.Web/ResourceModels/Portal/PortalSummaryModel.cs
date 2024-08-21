// <copyright file="PortalSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Portal
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Portal;

    public class PortalSummaryModel
    {
        public PortalSummaryModel(PortalReadModelSummary portal)
        {
            this.Id = portal.Id;
            this.TenantId = portal.TenantId;
            this.UserType = portal.UserType;
            this.OrganisationId = portal.OrganisationId;
            this.Name = portal.Name;
            this.Alias = portal.Alias;
            this.Title = portal.Title;
            this.Deleted = portal.Deleted;
            this.Disabled = portal.Disabled;
            this.IsDefault = portal.IsDefault;
            this.CreatedDateTime = portal.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = portal.CreatedTimestamp.ToExtendedIso8601String();
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public Guid TenantId { get; private set; }

        [JsonProperty]
        public PortalUserType UserType { get; private set; }

        [JsonProperty]
        public Guid? OrganisationId { get; set; }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Alias { get; private set; }

        [JsonProperty]
        public string Title { get; private set; }

        [JsonProperty]
        public bool Deleted { get; private set; }

        [JsonProperty]
        public bool Disabled { get; private set; }

        [JsonProperty]
        public bool IsDefault { get; private set; }

        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }
    }
}
