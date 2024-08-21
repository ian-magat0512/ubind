// <copyright file="Person.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using DomainModelFields = UBind.Domain.ReadModel.Person.Fields;
    using Fields = UBind.Domain.Aggregates.Person.Fields;
    using ValueTypes = UBind.Domain.ValueTypes;

    /// <summary>
    /// This class is needed because we need to generate json representation of person entity that conforms with
    /// serialized-entity-schema.json file.
    /// </summary>
    public class Person : BaseEntity<PersonReadModel>
    {
        public Person(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        /// <param name="person">The person entity.</param>
        public Person(PersonReadModel person)
            : base(
                  person.Id,
                  person.CreatedTicksSinceEpoch,
                  person.LastModifiedTicksSinceEpoch)
        {
            this.PreferredName = this.VerifyValue(person.PreferredName);
            this.FirstName = this.VerifyValue(person.FirstName);
            this.MiddleNames = this.VerifyValue(person.MiddleNames);
            this.LastName = this.VerifyValue(person.LastName);
            this.NamePrefix = this.VerifyValue(person.NamePrefix);
            this.NameSuffix = this.VerifyValue(person.NameSuffix);
            this.DisplayName = this.VerifyValue(person.DisplayName);
            this.FullName = this.GetFullName();
            this.GreetingName = this.PreferredName.IsNotNullOrWhitespace() ? this.PreferredName : this.FirstName;
            this.TenantId = person.TenantId.ToString();
            this.Company = this.VerifyValue(person.Company);
            this.Title = this.VerifyValue(person.Title);
            this.EmailAddresses = person.EmailAddresses?
                .OrderBy(x => x.SequenceNo)?
                .Select(ea => new EmailAddress(ea))?
                .ToList() ??
                new List<EmailAddress>();
            this.PhoneNumbers = person.PhoneNumbers?
                .OrderBy(x => x.SequenceNo)?
                .Select(pn => new PhoneNumber(pn))?
                .ToList() ??
                new List<PhoneNumber>();
            this.StreetAddresses = person.StreetAddresses?
                .OrderBy(x => x.SequenceNo)?
                .Select(sa => new StreetAddress(sa))?.ToList() ??
                new List<StreetAddress>();
            this.WebsiteAddresses = person.WebsiteAddresses?
                .OrderBy(x => x.SequenceNo)?
                .Select(wa => new WebsiteAddress(wa))?.ToList() ??
                new List<WebsiteAddress>();
            this.MessengerIds = person.MessengerIds?
                .OrderBy(x => x.SequenceNo)?
                .Select(mi => new MessengerId(mi))?.ToList() ??
                new List<MessengerId>();
            this.SocialMediaIds = person.SocialMediaIds?
                .OrderBy(x => x.SequenceNo)?
                .Select(sm => new SocialMediaId(sm))?.ToList() ??
                new List<SocialMediaId>();
            if (person.OrganisationId != (Guid)default)
            {
                this.OrganisationId = person.OrganisationId.ToString();
            }

            if (person.CustomerId.HasValue)
            {
                this.CustomerId = person.CustomerId.ToString();
            }

            if (person.UserId.HasValue)
            {
                this.UserId = person.UserId.ToString();
            }

            this.TestData = person.IsTestData;

            this.AppendOlderFieldsToRepeatingSets(person);
            this.SetFirstFieldToDefault();

            // these constructors are necessary to remove references from the list.
            var defaultEmailAddress = this.EmailAddresses.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultEmailAddress = defaultEmailAddress != null ? new EmailAddress(defaultEmailAddress) : null;
            var defaultPhoneNumber = this.PhoneNumbers.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultPhoneNumber = defaultPhoneNumber != null ? new PhoneNumber(defaultPhoneNumber) : null;
            var defaultStreetAddress = this.StreetAddresses.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultStreetAddress = defaultStreetAddress != null ? new StreetAddress(defaultStreetAddress) : null;
            var defaultWebsiteAddress = this.WebsiteAddresses.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultWebsiteAddress = defaultWebsiteAddress != null ? new WebsiteAddress(defaultWebsiteAddress) : null;
            var defaultMessengerId = this.MessengerIds.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultMessengerId = defaultMessengerId != null ? new MessengerId(defaultMessengerId) : null;
            var defaultSocialMediaId = this.SocialMediaIds.FirstOrDefault(x => x.IsDefault != null && x.IsDefault.Value);
            this.DefaultSocialMediaId = defaultSocialMediaId != null ? new SocialMediaId(defaultSocialMediaId) : null;
            this.SetDefault(this.DefaultEmailAddress);
            this.SetDefault(this.DefaultPhoneNumber);
            this.SetDefault(this.DefaultStreetAddress);
            this.SetDefault(this.DefaultWebsiteAddress);
            this.SetDefault(this.DefaultMessengerId);
            this.SetDefault(this.DefaultSocialMediaId);
        }

        public Person(IPersonReadModelWithRelatedEntities model, IEnumerable<string> includedProperties)
            : this(model.Person)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            if (model.Organisation != null)
            {
                this.Organisation = new Organisation(model.Organisation);
            }

            var ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;

            if (model.User != null)
            {
                // Automatically assigns the user Id
                this.User = new User(model.User);
            }
            else if (includedProperties.Contains("user", ordinalIgnoreCase) && this.User == null)
            {
                this.UserId = Guid.Empty.ToString();
            }

            if (model.Customer != null)
            {
                // Automatically assigns the customer Id
                this.Customer = new Customer(model.Customer);
            }
            else if (includedProperties.Contains("customer", ordinalIgnoreCase) && this.Customer == null)
            {
                this.CustomerId = Guid.Empty.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        [JsonConstructor]
        public Person()
        {
        }

        /// <summary>
        /// Gets or sets the tenant ID of the person.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 17)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the person.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 18)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organization id.
        /// </summary>
        [JsonProperty(PropertyName = "organisationId", Order = 19)]
        [Required]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organization associated with the person.
        /// </summary>
        [JsonProperty(PropertyName = "organisation", Order = 20)]
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the display name of the person.
        /// </summary>
        [JsonProperty("displayName", Order = 21)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        [JsonProperty("fullName", Order = 22)]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the greeting name of the person.
        /// </summary>
        [JsonProperty("greetingName", Order = 23)]
        public string GreetingName { get; set; }

        /// <summary>
        /// Gets or sets the name prefix of the person.
        /// </summary>
        [JsonProperty("namePrefix", Order = 24)]
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the first name of the person.
        /// </summary>
        [JsonProperty("firstName", Order = 25)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle names of the person.
        /// </summary>
        [JsonProperty("middleNames", Order = 26)]
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        [JsonProperty("lastName", Order = 27)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the name suffix of the person.
        /// </summary>
        [JsonProperty("nameSuffix", Order = 28)]
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the preferred name of the person.
        /// </summary>
        [JsonProperty("preferredName", Order = 29)]
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the company of the person.
        /// </summary>
        [JsonProperty("company", Order = 30)]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the title of the person.
        /// </summary>
        [JsonProperty("title", Order = 31)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the list of phone numbers associated with this person.
        /// </summary>
        [JsonProperty("phoneNumbers", Order = 32)]
        public List<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the list of phone numbers associated with this person.
        /// </summary>
        [JsonProperty("defaultPhoneNumber", Order = 33)]
        public PhoneNumber DefaultPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of email addresses associated with this person.
        /// </summary>
        [JsonProperty("emailAddresses", Order = 34)]
        public List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of email addresses associated with this person.
        /// </summary>
        [JsonProperty("defaultEmailAddress", Order = 35)]
        public EmailAddress DefaultEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of street addresses associated with this person.
        /// </summary>
        [JsonProperty("streetAddresses", Order = 36)]
        public List<StreetAddress> StreetAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of street addresses associated with this person.
        /// </summary>
        [JsonProperty("defaultStreetAddress", Order = 37)]
        public StreetAddress DefaultStreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of website addresses associated with this person.
        /// </summary>
        [JsonProperty("websiteAddresses", Order = 38)]
        public List<WebsiteAddress> WebsiteAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of website addresses associated with this person.
        /// </summary>
        [JsonProperty("defaultWebsiteAddress", Order = 39)]
        public WebsiteAddress DefaultWebsiteAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of Messenger IDs associated with this person.
        /// </summary>
        [JsonProperty("messengerIds", Order = 40)]
        public List<MessengerId> MessengerIds { get; set; }

        /// <summary>
        /// Gets or sets the list of Messenger IDs associated with this person.
        /// </summary>
        [JsonProperty("defaultMessengerId", Order = 41)]
        public MessengerId DefaultMessengerId { get; set; }

        /// <summary>
        /// Gets or sets the list of social media IDs associated with this person.
        /// </summary>
        [JsonProperty("socialMediaIds", Order = 42)]
        public List<SocialMediaId> SocialMediaIds { get; set; }

        /// <summary>
        /// Gets or sets the list of social media IDs associated with this person.
        /// </summary>
        [JsonProperty("defaultSocialMediaIds", Order = 43)]
        public SocialMediaId DefaultSocialMediaId { get; set; }

        /// <summary>
        /// Gets or sets the ID (UUID) of the user account associated with this person, if they have one.
        /// </summary>
        [JsonProperty(PropertyName = "userId", Order = 44)]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user account associated with this person, if they have one.
        /// </summary>
        [JsonProperty(PropertyName = "user", Order = 45)]
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the ID (UUID) of the customer associated with this person, if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "customerId", Order = 46)]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID (UUID) of the customer associated with this person, if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "customer", Order = 47)]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is tagged as test data.
        /// </summary>
        [JsonProperty(PropertyName = "testData", Order = 48)]
        [DefaultValue(false)]
        public bool? TestData { get; set; }

        private void SetFirstFieldToDefault()
        {
            if (this.EmailAddresses != null &&
                this.EmailAddresses.Any()
                && !this.EmailAddresses.Any(x => x.IsDefault.Value))
            {
                this.EmailAddresses.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.EmailAddresses[0].IsDefault = true;
            }

            if (this.PhoneNumbers != null &&
                this.PhoneNumbers.Any()
                && !this.PhoneNumbers.Any(x => x.IsDefault.Value))
            {
                this.PhoneNumbers.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.PhoneNumbers[0].IsDefault = true;
            }

            if (this.StreetAddresses != null &&
                this.StreetAddresses.Any() &&
                !this.StreetAddresses.Any(x => x.IsDefault.Value))
            {
                this.StreetAddresses.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.StreetAddresses[0].IsDefault = true;
            }

            if (this.WebsiteAddresses != null &&
                this.WebsiteAddresses.Any()
                && !this.WebsiteAddresses.Any(x => x.IsDefault.Value))
            {
                this.WebsiteAddresses.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.WebsiteAddresses[0].IsDefault = true;
            }

            if (this.MessengerIds != null &&
                this.MessengerIds.Any() &&
                !this.MessengerIds.Any(x => x.IsDefault.Value))
            {
                this.MessengerIds.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.MessengerIds[0].IsDefault = true;
            }

            if (this.SocialMediaIds != null &&
                this.SocialMediaIds.Any() &&
                !this.SocialMediaIds.Any(x => x.IsDefault.Value))
            {
                this.SocialMediaIds.ForEach((x) =>
                {
                    x.IsDefault = null;
                });
                this.SocialMediaIds[0].IsDefault = true;
            }
        }

        private string VerifyValue(string value)
        {
            return value.IsNullOrEmpty() ? null : value;
        }

        private void AppendOlderFieldsToRepeatingSets(PersonReadModel person)
        {
            this.EmailAddresses ??= new List<EmailAddress>();
            var existingEmailAddresses = this.EmailAddresses.Where(x => x.Email != null);
            var existingPhoneNumbers = this.PhoneNumbers.Where(x => x.ContactNumber != null);
            if (!existingEmailAddresses.Any(x => x.Email.Equals(person.Email))
                && person.Email.IsNotNullOrEmpty())
            {
                this.EmailAddresses.Add(new EmailAddress(
                    new DomainModelFields.EmailAddressReadModel(
                        person.TenantId,
                        new Fields.EmailAddressField(
                            "personal", string.Empty, new ValueTypes.EmailAddress(person.Email)))));
            }

            if (!existingEmailAddresses.Any(x => x.Email.Equals(person.AlternativeEmail))
                && person.AlternativeEmail.IsNotNullOrEmpty())
            {
                this.EmailAddresses.Add(new EmailAddress(
                    new DomainModelFields.EmailAddressReadModel(
                        person.TenantId,
                        new Fields.EmailAddressField(
                            "work", string.Empty, new ValueTypes.EmailAddress(person.AlternativeEmail)))));
            }

            if (!existingPhoneNumbers.Any(x => x.ContactNumber.Equals(person.HomePhoneNumber))
                && person.HomePhoneNumber.IsNotNullOrEmpty())
            {
                this.PhoneNumbers.Add(new PhoneNumber(
                    new DomainModelFields.PhoneNumberReadModel(
                        person.TenantId,
                        new Fields.PhoneNumberField(
                            "home", string.Empty, new ValueTypes.PhoneNumber(person.HomePhoneNumber)))));
            }

            if (!existingPhoneNumbers.Any(x => x.ContactNumber.Equals(person.WorkPhoneNumber))
                && person.WorkPhoneNumber.IsNotNullOrEmpty())
            {
                this.PhoneNumbers.Add(new PhoneNumber(
                    new DomainModelFields.PhoneNumberReadModel(
                        person.TenantId,
                        new Fields.PhoneNumberField(
                            "work", string.Empty, new ValueTypes.PhoneNumber(person.WorkPhoneNumber)))));
            }

            if (!existingPhoneNumbers.Any(x => x.ContactNumber.Equals(person.MobilePhoneNumber))
                && person.MobilePhoneNumber.IsNotNullOrEmpty())
            {
                this.PhoneNumbers.Add(new PhoneNumber(
                    new DomainModelFields.PhoneNumberReadModel(
                        person.TenantId,
                        new Fields.PhoneNumberField(
                            "mobile", string.Empty, new ValueTypes.PhoneNumber(person.MobilePhoneNumber)))));
            }
        }

        /// <summary>
        /// Forms the full name based on the name components.
        /// </summary>
        /// <returns>The person's full name.</returns>
        /// <remarks>Only call this after the name components have been assigned.</remarks>
        private string GetFullName()
        {
            var prefix = this.NamePrefix.IsNotNullOrWhitespace() ? $"{this.NamePrefix} " : string.Empty;
            var preferredName = this.PreferredName.IsNotNullOrWhitespace() ? $" ({this.PreferredName})" : string.Empty;
            var midName = this.MiddleNames.IsNotNullOrWhitespace() ? $" {this.MiddleNames}" : string.Empty;
            var suffix = this.NameSuffix.IsNotNullOrWhitespace() ? $" {this.NameSuffix}" : string.Empty;
            return string.Concat(
                prefix,
                $"{this.FirstName}",
                preferredName,
                midName,
                $" {this.LastName}",
                suffix);
        }

        /// <summary>
        /// Setting default to null will show up as the field is empty in the json.
        /// </summary>
        /// <param name="field">The field.</param>
        private void SetDefault(OrderedField? field)
        {
            if (field != null)
            {
                field.IsDefault = null;
            }
        }
    }
}
