// <copyright file="PersonConstructor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.ContactDetail
{
    using System.Collections.Generic;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents the person reference for the automation.
    /// </summary>
    public class PersonConstructor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonConstructor"/> class.
        /// </summary>
        public PersonConstructor(
            IProvider<Data<string>> firstName,
            IProvider<Data<string>>? namePrefix = null,
            IProvider<Data<string>>? lastName = null,
            IProvider<Data<string>>? middleNames = null,
            IProvider<Data<string>>? nameSuffix = null,
            IProvider<Data<string>>? preferredName = null,
            IProvider<Data<string>>? company = null,
            IProvider<Data<string>>? title = null,
            IEnumerable<PhoneNumberConstructor>? phoneNumbers = null,
            IEnumerable<EmailAddressConstructor>? emailAddresses = null,
            IEnumerable<StreetAddressConstructor>? streetAddresses = null,
            IEnumerable<WebsiteAddressConstructor>? websiteAddresses = null,
            IEnumerable<MessengerIdConstructor>? messengerIds = null,
            IEnumerable<SocialMediaIdConstructor>? socialMediaIds = null)
        {
            this.FirstName = firstName;
            this.NamePrefix = namePrefix;
            this.LastName = lastName;
            this.MiddleNames = middleNames;
            this.NameSuffix = nameSuffix;
            this.PreferredName = preferredName;
            this.Company = company;
            this.Title = title;
            this.PhoneNumbers = phoneNumbers;
            this.EmailAddresses = emailAddresses;
            this.StreetAddresses = streetAddresses;
            this.WebsiteAddresses = websiteAddresses;
            this.MessengerIds = messengerIds;
            this.SocialMediaIds = socialMediaIds;
        }

        public IProvider<Data<string>>? NamePrefix { get; }

        public IProvider<Data<string>> FirstName { get; }

        public IProvider<Data<string>>? LastName { get; }

        public IProvider<Data<string>>? MiddleNames { get; }

        public IProvider<Data<string>>? NameSuffix { get; }

        public IProvider<Data<string>>? PreferredName { get; }

        public IProvider<Data<string>>? Company { get; }

        public IProvider<Data<string>>? Title { get; }

        public IEnumerable<PhoneNumberConstructor>? PhoneNumbers { get; }

        public IEnumerable<EmailAddressConstructor>? EmailAddresses { get; }

        public IEnumerable<StreetAddressConstructor>? StreetAddresses { get; }

        public IEnumerable<WebsiteAddressConstructor>? WebsiteAddresses { get; }

        public IEnumerable<MessengerIdConstructor>? MessengerIds { get; }

        public IEnumerable<SocialMediaIdConstructor>? SocialMediaIds { get; }

        public async Task<PersonConstructorData> Resolve(IProviderContext providerContext)
        {
            var firstName = (await this.FirstName.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var namePrefix = (await this.NamePrefix.ResolveValueIfNotNull(providerContext))?.DataValue;
            var lastName = (await this.LastName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var middleNames = (await this.MiddleNames.ResolveValueIfNotNull(providerContext))?.DataValue;
            var nameSuffix = (await this.NameSuffix.ResolveValueIfNotNull(providerContext))?.DataValue;
            var preferredName = (await this.PreferredName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var company = (await this.Company.ResolveValueIfNotNull(providerContext))?.DataValue;
            var title = (await this.Title.ResolveValueIfNotNull(providerContext))?.DataValue;
            var phoneNumbers = this.PhoneNumbers != null ? await this.PhoneNumbers.SelectAsync(x => x.Resolve(providerContext)) : new List<PhoneNumberConstructorData>();
            var emailAddresses = this.EmailAddresses != null ? await this.EmailAddresses.SelectAsync(x => x.Resolve(providerContext)) : new List<EmailAddressConstructorData>();
            var streetAddresses = this.StreetAddresses != null ? await this.StreetAddresses.SelectAsync(x => x.Resolve(providerContext)) : new List<StreetAddressConstructorData>();
            var websiteAddresses = this.WebsiteAddresses != null ? await this.WebsiteAddresses.SelectAsync(x => x.Resolve(providerContext)) : new List<WebsiteAddressConstructorData>();
            var messengerIds = this.MessengerIds != null ? await this.MessengerIds.SelectAsync(x => x.Resolve(providerContext)) : new List<MessengerIdConstructorData>();
            var socialMediaIds = this.SocialMediaIds != null ? await this.SocialMediaIds.SelectAsync(x => x.Resolve(providerContext)) : new List<SocialMediaIdConstructorData>();
            return new PersonConstructorData(
                namePrefix,
                firstName,
                lastName,
                middleNames,
                nameSuffix,
                preferredName,
                company,
                title,
                phoneNumbers,
                emailAddresses,
                streetAddresses,
                websiteAddresses,
                messengerIds,
                socialMediaIds);
        }
    }
}
