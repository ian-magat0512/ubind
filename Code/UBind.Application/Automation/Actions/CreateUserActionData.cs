// <copyright file="CreateUserActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class CreateUserActionData : ActionData
    {
        public CreateUserActionData(
            string name,
            string alias,
            IClock clock)
            : base(name, alias, ActionType.CreateUserAction, clock)
        {
        }

        [JsonConstructor]
        public CreateUserActionData()
            : base(ActionType.CreateUserAction)
        {
        }

        [JsonProperty("organisationId")]
        public Guid? OrganisationId { get; set; }

        [JsonProperty("accountEmailAddress")]
        public string? AccountEmailAddress { get; set; }

        [JsonProperty(PropertyName = "additionalProperties", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyDictionary<string, object>? AdditionalProperties { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; internal set; }

        [JsonProperty(PropertyName = "portalId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? PortalId { get; internal set; }

        public void SetFields(Guid userId, Guid? organisationId, string accountEmailAddress, ReadOnlyDictionary<string, object>? additionalProperties, Guid? portalId)
        {
            this.OrganisationId = organisationId;
            this.AccountEmailAddress = accountEmailAddress;
            this.UserId = userId;
            if (portalId != null)
            {
                this.PortalId = portalId;
            }

            if (additionalProperties != null)
            {
                this.AdditionalProperties = additionalProperties;
            }
        }
    }
}
