// <copyright file="CustomerImportData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Represents a customer import object.
    /// </summary>
    public class CustomerImportData : IPersonalDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerImportData"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the customer is to be imported for.</param>
        /// <param name="organisationId">The ID of the organisation the customer is to be imported for.</param>
        /// <param name="jObject">Represents a JSON object.</param>
        /// <param name="mapping">Represents the customer mapping definition.</param>
        public CustomerImportData(Guid tenantId, Guid organisationId, JObject jObject, CustomerMapping mapping)
        {
            if (jObject == null || mapping == null)
            {
                return;
            }

            this.FullName = jObject.Value<string>(mapping.FullName);
            this.NamePrefix = jObject.Value<string>(mapping.NamePrefix);
            this.FirstName = jObject.Value<string>(mapping.FirstName);
            this.MiddleNames = jObject.Value<string>(mapping.MiddleNames);
            this.LastName = jObject.Value<string>(mapping.LastName);
            this.NameSuffix = jObject.Value<string>(mapping.NameSuffix);
            this.PreferredName = jObject.Value<string>(mapping.PreferredName);
            this.Email = jObject.Value<string>(mapping.Email);

            this.AlternativeEmail = jObject.Value<string>(mapping.AlternativeEmail);
            this.MobilePhone = jObject.Value<string>(mapping.MobilePhone);
            this.HomePhone = jObject.Value<string>(mapping.HomePhone);
            this.WorkPhone = jObject.Value<string>(mapping.WorkPhone);
            this.Company = jObject.Value<string>(mapping.Company);
            this.Title = jObject.Value<string>(mapping.Title);
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;

            if (this.FirstName.IsNullOrEmpty() || this.LastName.IsNullOrEmpty())
            {
                var personCommonProperties = new PersonCommonProperties
                {
                    FullName = jObject.Value<string>(mapping.FullName),
                };

                personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

                this.FirstName = personCommonProperties.FirstName;
                this.MiddleNames = personCommonProperties.MiddleNames;
                this.LastName = personCommonProperties.LastName;
            }

            this.DefineEmailAddresses();
            this.DefinePhoneNumbers();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerImportData"/> class.
        /// This is used for policy creation, with minimal requirements.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the customer is to be imported for.</param>
        /// <param name="organisationId">The ID of the organisation the customer is to be imported for.</param>
        /// <param name="email">The customer email.</param>
        /// <param name="fullname">The customer full name.</param>
        public CustomerImportData(Guid tenantId, Guid organisationId, string email, string fullname)
        {
            this.Email = email;
            this.FullName = fullname;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            var personCommonProperties = new PersonCommonProperties
            {
                FullName = fullname,
            };
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;

            this.DefineEmailAddresses();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerImportData"/> class.
        /// </summary>
        [JsonConstructor]
        public CustomerImportData()
        {
        }

        /// <summary>
        /// Gets the name property of the customer object.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the preferred name property of the customer object.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the email property of the customer object.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the alternative email property of the customer object.
        /// </summary>
        [JsonProperty]
        public string AlternativeEmail { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the mobile phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string MobilePhone { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the home phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string HomePhone { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the work phone property of the customer object.
        /// </summary>
        [JsonProperty]
        public string WorkPhone { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public Guid TenantId { get; private set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string NamePrefix { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string FirstName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string MiddleNames { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string LastName { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string NameSuffix { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string Company { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string Title { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty]
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

        /// <inheritdoc/>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "emailAddresses")]
        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "streetAddresses")]
        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "phoneNumbers")]
        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "messengerIds")]
        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "websiteAddresses")]
        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "socialMediaIds")]
        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

        /// <inheritdoc/>
        public string MobilePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string HomePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string WorkPhoneNumber { get; set; }

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
            throw new NotImplementedException();
        }

        private void DefineEmailAddresses()
        {
            var emailAddresses = new List<EmailAddressField>();

            var primaryEmail = new EmailAddressField()
            {
                EmailAddress = this.Email,
                Label = "personal",
                CustomLabel = null,
                SequenceNo = 0,
                Id = default,
            };
            emailAddresses.Add(primaryEmail);

            if (!this.AlternativeEmail.IsNullOrWhitespace())
            {
                var alternativeEmailEntries = this.AlternativeEmail?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                int ctr = 1;
                foreach (var alt in alternativeEmailEntries)
                {
                    var alternativeEmail = new EmailAddressField()
                    {
                        EmailAddress = alt,
                        Label = "work",
                        CustomLabel = null,
                        SequenceNo = ctr,
                        Id = default,
                    };
                    emailAddresses.Add(alternativeEmail);
                    ctr++;
                }

                this.AlternativeEmail = null;
            }

            this.EmailAddresses = emailAddresses;
        }

        private void DefinePhoneNumbers()
        {
            var phoneNumbers = new List<PhoneNumberField>();
            if (this.HomePhone.IsNotNullOrEmpty())
            {
                var homePhone = new PhoneNumberField()
                {
                    PhoneNumber = this.HomePhone,
                    Label = "home",
                    CustomLabel = null,
                    SequenceNo = phoneNumbers.Any() ?
                    phoneNumbers.Count - 1 :
                    0,
                    Id = default,
                };
                phoneNumbers.Add(homePhone);
                this.HomePhone = null;
            }

            if (this.MobilePhone.IsNotNullOrEmpty())
            {
                var mobile = new PhoneNumberField()
                {
                    PhoneNumber = this.MobilePhone,
                    Label = "mobile",
                    CustomLabel = null,
                    SequenceNo = phoneNumbers.Any() ?
                    phoneNumbers.Count - 1 :
                    0,
                    Id = default,
                };
                phoneNumbers.Add(mobile);
                this.MobilePhone = null;
            }

            if (this.WorkPhone.IsNotNullOrEmpty())
            {
                var work = new PhoneNumberField()
                {
                    PhoneNumber = this.WorkPhone,
                    Label = "work",
                    CustomLabel = null,
                    SequenceNo = phoneNumbers.Any() ?
                    phoneNumbers.Count - 1 :
                    0,
                    Id = default,
                };
                phoneNumbers.Add(work);
                this.WorkPhone = null;
            }

            this.PhoneNumbers = phoneNumbers;
        }
    }
}
