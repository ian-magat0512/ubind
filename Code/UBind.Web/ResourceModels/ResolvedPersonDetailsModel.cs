// <copyright file="ResolvedPersonDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Helpers;
    using UBind.Web.ResourceModels.Person;

    public class ResolvedPersonDetailsModel : IPersonalDetails
    {
        public ResolvedPersonDetailsModel(Tenant tenant, Guid organisationId, PersonDetailsModel model)
        {
            this.TenantId = tenant.Id;
            this.OrganisationId = organisationId;
            this.FullName = model.FullName;
            this.PreferredName = model.PreferredName;
            this.Email = model.Email;
            this.AlternativeEmail = model.AlternativeEmail;
            this.MobilePhone = model.MobilePhone;
            this.HomePhone = model.HomePhone;
            this.WorkPhone = model.WorkPhone;
            this.NamePrefix = model.NamePrefix;
            this.FirstName = model.FirstName;
            this.MiddleNames = model.MiddleNames;
            this.LastName = model.LastName;
            this.NameSuffix = model.NameSuffix;
            this.Company = model.Company;
            this.Title = model.Title;
            this.EmailAddresses = model.EmailAddresses;
            this.PhoneNumbers = model.PhoneNumbers;
            this.StreetAddresses = model.StreetAddresses;
            this.WebsiteAddresses = model.WebsiteAddresses;
            this.MessengerIds = model.MessengerIds;
            this.SocialMediaIds = model.SocialMediaIds;
        }

        public ResolvedPersonDetailsModel(Tenant tenant, Guid organisationId, PersonUpsertModel model)
        {
            this.TenantId = tenant.Id;
            this.OrganisationId = organisationId;
            this.FullName = model.FullName;
            this.PreferredName = model.PreferredName;
            this.Email = model.Email;
            this.AlternativeEmail = model.AlternativeEmail;
            this.MobilePhone = model.MobilePhone;
            this.HomePhone = model.HomePhone;
            this.WorkPhone = model.WorkPhone;
            this.NamePrefix = model.NamePrefix;
            this.FirstName = model.FirstName;
            this.MiddleNames = model.MiddleNames;
            this.LastName = model.LastName;
            this.NameSuffix = model.NameSuffix;
            this.Company = model.Company;
            this.Title = model.Title;
            this.EmailAddresses = model.EmailAddresses;
            this.PhoneNumbers = model.PhoneNumbers;
            this.StreetAddresses = model.StreetAddresses;
            this.WebsiteAddresses = model.WebsiteAddresses;
            this.MessengerIds = model.MessengerIds;
            this.SocialMediaIds = model.SocialMediaIds;
        }

        private ResolvedPersonDetailsModel()
        {
        }

        [JsonProperty]
        public Guid TenantId { get; private set; }

        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        public string MobilePhoneNumber { get; set; }

        public string HomePhoneNumber { get; set; }

        public string WorkPhoneNumber { get; set; }

        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

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
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

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

        /// <inheritdoc/>
        public void SetEmail(string email)
        {
            this.Email = email;
        }

        /// <inheritdoc/>
        public void SetAlternativeEmail(string email)
        {
            this.AlternativeEmail = email;
        }

        /// <inheritdoc/>
        public void SetEmailIfNull()
        {
            PersonDetailExtension.SetEmailIfNull(this);
        }
    }
}
