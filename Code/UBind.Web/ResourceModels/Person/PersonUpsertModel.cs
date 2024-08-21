// <copyright file="PersonUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;

    public class PersonUpsertModel
    {
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        [JsonProperty]
        public Guid CustomerId { get; private set; }

        [JsonProperty]
        public Guid? PortalId { get; private set; }

        [JsonProperty]
        public string FullName { get; private set; }

        [JsonProperty]
        public string NamePrefix { get; private set; }

        [JsonProperty]
        public string FirstName { get; private set; }

        [JsonProperty]
        public string MiddleNames { get; private set; }

        [JsonProperty]
        public string LastName { get; private set; }

        [JsonProperty]
        public string DisplayName { get; private set; }

        [JsonProperty]
        public string NameSuffix { get; private set; }

        [JsonProperty]
        public string Company { get; private set; }

        [JsonProperty]
        public string Title { get; private set; }

        [JsonProperty]
        public string PreferredName { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        [JsonProperty]
        public string AlternativeEmail { get; private set; }

        [JsonProperty]
        public string MobilePhone { get; private set; }

        [JsonProperty]
        public string HomePhone { get; private set; }

        [JsonProperty]
        public string WorkPhone { get; private set; }

        [JsonProperty]
        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        [JsonProperty]
        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        [JsonProperty]
        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        [JsonProperty]
        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        [JsonProperty]
        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        [JsonProperty]
        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();
    }
}
