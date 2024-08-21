// <copyright file="PersonConstructorConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.ContactDetail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    public class PersonConstructorConfigModel : IBuilder<PersonConstructor>
    {
        [JsonConstructor]
        public PersonConstructorConfigModel(
            IBuilder<IProvider<Data<string>>> firstName,
            IBuilder<IProvider<Data<string>>> namePrefix,
            IBuilder<IProvider<Data<string>>> middleNames,
            IBuilder<IProvider<Data<string>>> lastName,
            IBuilder<IProvider<Data<string>>> nameSuffix,
            IBuilder<IProvider<Data<string>>> preferredName,
            IBuilder<IProvider<Data<string>>> company,
            IBuilder<IProvider<Data<string>>> title,
            IEnumerable<PhoneNumberConstructorConfigModel> phoneNumbers,
            IEnumerable<EmailAddressConstructorConfigModel> emailAddresses,
            IEnumerable<StreetAddressConstructorConfigModel> streetAddresses,
            IEnumerable<WebsiteAddressConstructorConfigModel> websiteAddresses,
            IEnumerable<MessengerIdConstructorConfigModel> messengerIds,
            IEnumerable<SocialMediaIdConstructorConfigModel> socialMediaIds)
        {
            this.FirstName = firstName;
            this.NamePrefix = namePrefix;
            this.MiddleNames = middleNames;
            this.LastName = lastName;
            this.NameSuffix = nameSuffix;
            this.PreferredName = preferredName;
            this.Company = company;
            this.Title = title;
            this.PhoneNumberConfigModels = phoneNumbers;
            this.EmailAddressConfigModels = emailAddresses;
            this.StreetAddressConfigModels = streetAddresses;
            this.WebsiteAddressConfigModels = websiteAddresses;
            this.MessengerIdConfigModels = messengerIds;
            this.SocialMediaIdConfigModels = socialMediaIds;
        }

        [JsonProperty("firstName")]
        public IBuilder<IProvider<Data<string>>> FirstName { get; set; }

        [JsonProperty("namePrefix")]
        public IBuilder<IProvider<Data<string>>> NamePrefix { get; set; }

        [JsonProperty("lastName")]
        public IBuilder<IProvider<Data<string>>> LastName { get; set; }

        [JsonProperty("middleName")]
        public IBuilder<IProvider<Data<string>>> MiddleNames { get; set; }

        [JsonProperty("nameSuffix")]
        public IBuilder<IProvider<Data<string>>> NameSuffix { get; set; }

        [JsonProperty("preferredName")]
        public IBuilder<IProvider<Data<string>>> PreferredName { get; set; }

        [JsonProperty("company")]
        public IBuilder<IProvider<Data<string>>> Company { get; set; }

        [JsonProperty("title")]
        public IBuilder<IProvider<Data<string>>> Title { get; set; }

        [JsonProperty("phoneNumbers")]
        public IEnumerable<PhoneNumberConstructorConfigModel> PhoneNumberConfigModels { get; }

        [JsonProperty("emailAddresses")]
        public IEnumerable<EmailAddressConstructorConfigModel> EmailAddressConfigModels { get; }

        [JsonProperty("streetAddresses")]
        public IEnumerable<StreetAddressConstructorConfigModel> StreetAddressConfigModels { get; }

        [JsonProperty("websiteAddresses")]
        public IEnumerable<WebsiteAddressConstructorConfigModel> WebsiteAddressConfigModels { get; }

        [JsonProperty("msessengerIds")]
        public IEnumerable<MessengerIdConstructorConfigModel> MessengerIdConfigModels { get; }

        [JsonProperty("socialMediaIds")]
        public IEnumerable<SocialMediaIdConstructorConfigModel> SocialMediaIdConfigModels { get; }

        /// <inheritdoc/>
        public PersonConstructor Build(IServiceProvider dependencyProvider)
        {
            return new PersonConstructor(
                this.FirstName.Build(dependencyProvider),
                this.NamePrefix?.Build(dependencyProvider),
                this.LastName?.Build(dependencyProvider),
                this.MiddleNames?.Build(dependencyProvider),
                this.NameSuffix?.Build(dependencyProvider),
                this.PreferredName?.Build(dependencyProvider),
                this.Company?.Build(dependencyProvider),
                this.Title?.Build(dependencyProvider),
                this.PhoneNumberConfigModels?.Select(x => x.Build(dependencyProvider)),
                this.EmailAddressConfigModels?.Select(x => x.Build(dependencyProvider)),
                this.StreetAddressConfigModels?.Select(x => x.Build(dependencyProvider)),
                this.WebsiteAddressConfigModels?.Select(x => x.Build(dependencyProvider)),
                this.MessengerIdConfigModels?.Select(x => x.Build(dependencyProvider)),
                this.SocialMediaIdConfigModels?.Select(x => x.Build(dependencyProvider)));
        }
    }
}
