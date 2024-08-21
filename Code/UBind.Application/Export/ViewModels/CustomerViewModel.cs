// <copyright file="CustomerViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Customer view model for Razor Templates to use.
    /// </summary>
    public class CustomerViewModel : PersonCommonProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerViewModel"/> class.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="customer">The customer details to present.</param>
        /// <param name="environment">The environment of the customer.</param>
        public CustomerViewModel(
            Guid customerId,
            IPersonalDetails customer,
            DeploymentEnvironment? environment = null)
        {
            this.Id = customerId;
            this.TenantId = customer.TenantId;
            this.Environment = Convert.ToString(environment);

            this.PreferredName = customer.PreferredName;
            this.NamePrefix = customer.NamePrefix;
            this.FirstName = customer.FirstName;
            this.MiddleNames = customer.MiddleNames;
            this.LastName = customer.LastName;
            this.NameSuffix = customer.NameSuffix;

            this.SetNameComponentsFromFullNameIfNoneAlreadySet();
            this.SetBasicFullName();

            this.GreetingName = customer.GetGreetingName();
            this.DisplayName = PersonPropertyHelper.GetDisplayName(customer);
            this.Company = customer.Company;
            this.Title = customer.Title;
            this.Email = customer.Email;
            this.AlternativeEmail = customer.AlternativeEmail;
            this.MobilePhone = customer.MobilePhone;
            this.HomePhone = customer.HomePhone;
            this.WorkPhone = customer.WorkPhone;
            this.EmailAddresses = customer.EmailAddresses;
            this.PhoneNumbers = customer.PhoneNumbers;
            this.StreetAddresses = customer.StreetAddresses;
            this.WebsiteAddresses = this.WebsiteAddresses;
            this.MessengerIds = this.MessengerIds;
            this.SocialMediaIds = this.SocialMediaIds;
            this.ExtractEmailsAndPhoneNumbersFromRepeatingFields();
        }

        /// <summary>
        /// Gets or sets the user's greeting name.
        /// </summary>
        public string GreetingName { get; set; }

        /// <summary>
        /// Gets a customers's deployment environment.
        /// </summary>
        public string Environment { get; private set; }

        /// <summary>
        /// Gets the customer's mobile phone number if specified, otherwise empty string.
        /// </summary>
        public string MobilePhone { get; private set; }

        /// <summary>
        /// Gets the customer's home phone number if specified, otherwise empty string.
        /// </summary>
        public string HomePhone { get; private set; }

        /// <summary>
        /// Gets the customer's work phone number if specified, otherwise empty string.
        /// </summary>
        public string WorkPhone { get; private set; }

        /// <summary>
        /// Gets or sets the person's list of email addresses.
        /// </summary>
        [JsonProperty(PropertyName = "emailAddresses")]
        public IEnumerable<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        /// <summary>
        /// Gets or sets the person's list of street addresses.
        /// </summary>
        [JsonProperty(PropertyName = "streetAddresses")]
        public IEnumerable<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        /// <summary>
        /// Gets or sets the person's list of phone numbers.
        /// </summary>
        [JsonProperty(PropertyName = "phoneNumbers")]
        public IEnumerable<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        /// <summary>
        /// Gets or sets the person's list of messenger Ids.
        /// </summary>
        [JsonProperty(PropertyName = "messengerIds")]
        public IEnumerable<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        /// <summary>
        /// Gets or sets the person's list of website addresses.
        /// </summary>
        [JsonProperty(PropertyName = "websiteAddresses")]
        public IEnumerable<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        /// <summary>
        /// Gets or sets the person's list of social media Ids.
        /// </summary>
        [JsonProperty(PropertyName = "socialMediaIds")]
        public IEnumerable<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

        private void ExtractEmailsAndPhoneNumbersFromRepeatingFields()
        {
            if (this.EmailAddresses.Any())
            {
                this.Email = this.EmailAddresses.FirstOrDefault().EmailAddressValueObject.ToString();
                if (this.EmailAddresses.Count() > 1)
                {
                    this.AlternativeEmail
                        = this.EmailAddresses.Skip(1).FirstOrDefault().EmailAddressValueObject.ToString();
                }
            }

            if (this.PhoneNumbers.Any())
            {
                var workPhone = this.PhoneNumbers.FirstOrDefault(p => p.Label == "work");
                if (workPhone != null)
                {
                    this.WorkPhone = workPhone.PhoneNumberValueObject.ToString();
                }

                var mobilePhone = this.PhoneNumbers.FirstOrDefault(p => p.Label == "mobile");
                if (mobilePhone != null)
                {
                    this.MobilePhone = mobilePhone.PhoneNumberValueObject.ToString();
                }

                var homePhone = this.PhoneNumbers.FirstOrDefault(p => p.Label == "home");
                if (homePhone != null)
                {
                    this.HomePhone = homePhone.PhoneNumberValueObject.ToString();
                }
            }
        }
    }
}
