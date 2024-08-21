// <copyright file="OrganisationEntitySettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models
{
    using System;
    using Newtonsoft.Json;

    public class OrganisationEntitySettings : IEntitySettings
    {
        [JsonProperty("allowCustomerSelfAccountCreation")]
        [Obsolete("This was removed in UB-9162 and will be removed in UB-9510.")]
        public bool AllowCustomerSelfAccountCreation { get; set; } = false;

        [JsonProperty("allowOrganisationRenewalInvitation")]
        public bool AllowOrganisationRenewalInvitation { get; set; } = true;
    }
}
