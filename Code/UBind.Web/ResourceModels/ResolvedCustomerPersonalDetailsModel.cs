// <copyright file="ResolvedCustomerPersonalDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Helpers;
    using UBind.Domain.ValueTypes;

    public class ResolvedCustomerPersonalDetailsModel : IPersonalDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedCustomerPersonalDetailsModel"/> class.
        /// </summary>
        /// <param name="organisationId">The organisation Id of the resolved customer personal details.</param>
        /// <param name="tenant">The tenant of the resolved customer personal details.</param>
        /// <param name="model">The customer personal details model.</param>
        /// <param name="portalId">The ID of the portal the customer would login to, if any.</param>
        public ResolvedCustomerPersonalDetailsModel(
            Guid organisationId, Tenant tenant, CustomerPersonalDetailsModel model, Guid? portalId = null)
        {
            this.OrganisationId = organisationId;
            this.TenantId = tenant.Id;
            this.Environment = model.Environment;
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
            this.Properties = model.Properties;
            this.PortalId = portalId;
            this.MobilePhoneNumber = model.MobilePhone;
            this.HomePhoneNumber = model.HomePhone;
            this.WorkPhoneNumber = model.WorkPhone;

            this.AssignEmailAddresses();
            this.AssignPhoneNumbers();
        }

        /// <summary>
        /// Gets or sets the customer's tenant ID.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets the customer's organisation ID.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; }

        /// <summary>
        /// Gets the customer's portal ID.
        /// </summary>
        [JsonProperty]
        public Guid? PortalId { get; }

        /// <summary>
        /// Gets or sets the customer's environment.
        /// </summary>
        [JsonProperty]
        public DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets the customer's full name.
        /// </summary>
        [JsonProperty]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the customer's preferred name.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets the customer email address.
        /// </summary>
        [JsonProperty]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the customer's alternative email address.
        /// </summary>
        [JsonProperty]
        public string AlternativeEmail { get; private set; }

        /// <summary>
        /// Gets the customer's mobile phone number.
        /// </summary>
        [JsonProperty]
        public string MobilePhone { get; private set; }

        /// <summary>
        /// Gets the customer's home phone number.
        /// </summary>
        [JsonProperty]
        public string HomePhone { get; private set; }

        /// <summary>
        /// Gets the customer's work phone number.
        /// </summary>
        [JsonProperty]
        public string WorkPhone { get; private set; }

        /// <summary>
        /// Gets or sets the user's name prefix.
        /// </summary>
        [JsonProperty]
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's middle names.
        /// </summary>
        [JsonProperty]
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        [JsonProperty]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's name suffix.
        /// </summary>
        [JsonProperty]
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the user's company.
        /// </summary>
        [JsonProperty]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the user's company.
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        /// Gets the user's first name and last name.
        /// </summary>
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

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

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

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

        private void AssignEmailAddresses()
        {
            var emailAddresses = new List<EmailAddressField>();
            if (this.EmailAddresses != null && this.EmailAddresses.Any())
            {
                emailAddresses.AddRange(this.EmailAddresses);
            }

            // Customers that doesn't have a user account needs to have at least 1 email (if available) on the repeating field
            if (!string.IsNullOrEmpty(this.Email) && emailAddresses.Count == 0)
            {
                emailAddresses.Add(
                    new EmailAddressField("personal", null, new EmailAddress(this.Email)));
            }

            if (!string.IsNullOrEmpty(this.AlternativeEmail) && emailAddresses.Count == 0)
            {
                emailAddresses.Add(
                    new EmailAddressField("work", null, new EmailAddress(this.AlternativeEmail)));
            }

            this.EmailAddresses = emailAddresses;
        }

        private void AssignPhoneNumbers()
        {
            var phoneNumbers = new List<PhoneNumberField>();
            if (this.PhoneNumbers != null && this.PhoneNumbers.Any())
            {
                foreach (var phoneNumber in this.PhoneNumbers)
                {
                    if (!phoneNumbers.Any(x => x.Equals(phoneNumber)))
                    {
                        phoneNumbers.Add(phoneNumber);
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.HomePhone))
            {
                var homePhone = new PhoneNumberField("home", null, new PhoneNumber(this.HomePhone));
                if (!phoneNumbers.Any(x => x.Equals(homePhone)))
                {
                    phoneNumbers.Add(homePhone);
                }
            }

            if (!string.IsNullOrEmpty(this.MobilePhone))
            {
                var mobilePhone = new PhoneNumberField("mobile", null, new PhoneNumber(this.MobilePhone));
                if (!phoneNumbers.Any(x => x.Equals(mobilePhone)))
                {
                    phoneNumbers.Add(mobilePhone);
                }
            }

            if (!string.IsNullOrEmpty(this.WorkPhone))
            {
                var workPhone = new PhoneNumberField("work", null, new PhoneNumber(this.WorkPhone));
                if (!phoneNumbers.Any(x => x.Equals(workPhone)))
                {
                    phoneNumbers.Add(workPhone);
                }
            }

            this.PhoneNumbers = phoneNumbers;
        }
    }
}
