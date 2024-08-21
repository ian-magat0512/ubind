// <copyright file="CustomerPersonalDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For representing customer details.
    /// </summary>
    public class CustomerPersonalDetailsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerPersonalDetailsModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for model binding.</remarks>
        [JsonConstructor]
        public CustomerPersonalDetailsModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerPersonalDetailsModel"/> class.
        /// </summary>
        /// <param name="clientInfo">The details for the new customer.</param>
        public CustomerPersonalDetailsModel(UBind.Application.Funding.Iqumulate.Response.Client clientInfo)
        {
            this.MobilePhone = clientInfo.MobileNumber;
            this.HomePhone = clientInfo.TelephoneNumber;
            this.Email = clientInfo.Email;

            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = clientInfo.LegalName;
            personCommonProperties.PreferredName = this.PreferredName;
            personCommonProperties.NamePrefix = this.NamePrefix;
            personCommonProperties.FirstName = this.FirstName;
            personCommonProperties.MiddleNames = this.MiddleNames;
            personCommonProperties.LastName = this.LastName;
            personCommonProperties.NameSuffix = this.NameSuffix;

            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            personCommonProperties.SetBasicFullName();

            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;
        }

        /// <summary>
        /// Gets or sets the customer's organisation ID.
        /// Note: this is for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the customer's organisation ID or Alias.
        /// </summary>
        [JsonProperty]
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets the customer's portal ID.
        /// Note: this is for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string PortalId { get; set; }

        /// <summary>
        /// Gets or sets the customer's portal ID or Alias.
        /// </summary>
        [JsonProperty]
        public string Portal { get; set; }

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
        /// Gets the customer's email.
        /// </summary>
        [JsonProperty]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the customer's alternative email.
        /// </summary>
        [JsonProperty]
        public string AlternativeEmail { get; private set; }

        /// <summary>
        /// Gets the customer's mobile number.
        /// </summary>
        [JsonProperty]
        public string MobilePhone { get; private set; }

        /// <summary>
        /// Gets the customer's home phone.
        /// </summary>
        [JsonProperty]
        public string HomePhone { get; private set; }

        /// <summary>
        /// Gets the customer's work phone.
        /// </summary>
        [JsonProperty]
        public string WorkPhone { get; private set; }

        /// <summary>
        /// Gets the customer's login email address.
        /// </summary>
        [JsonProperty]
        public string LoginEmail { get; private set; }

        /// <summary>
        /// Gets or sets the customer's name prefix.
        /// </summary>
        [JsonProperty]
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the customer's first name.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the customer's middle name.
        /// </summary>
        [JsonProperty]
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the customer's last name.
        /// </summary>
        [JsonProperty]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the customer's name suffix.
        /// </summary>
        [JsonProperty]
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the customer's company.
        /// </summary>
        [JsonProperty]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the customer's title.
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets an enumerable of customer's email addresses.
        /// </summary>
        [JsonProperty(PropertyName = "emailAddresses")]
        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        /// <summary>
        /// Gets or sets an enumerable of customer's street addresses.
        /// </summary>
        [JsonProperty(PropertyName = "streetAddresses")]
        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        /// <summary>
        /// Gets or sets an enumerable of customer's phone numbers.
        /// </summary>
        [JsonProperty(PropertyName = "phoneNumbers")]
        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        /// <summary>
        /// Gets or sets an enumerable of customer's messenger Ids.
        /// </summary>
        [JsonProperty(PropertyName = "messengerIds")]
        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        /// <summary>
        /// Gets or sets an enumerable of customer's website addresses.
        /// </summary>
        [JsonProperty(PropertyName = "websiteAddresses")]
        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        /// <summary>
        /// Gets or sets an enumerable of customer's social media Ids.
        /// </summary>
        [JsonProperty(PropertyName = "socialMediaIds")]
        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

        public void FixNameComponents()
        {
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = this.FullName;
            personCommonProperties.PreferredName = this.PreferredName;
            personCommonProperties.NamePrefix = this.NamePrefix;
            personCommonProperties.FirstName = this.FirstName;
            personCommonProperties.MiddleNames = this.MiddleNames;
            personCommonProperties.LastName = this.LastName;
            personCommonProperties.NameSuffix = this.NameSuffix;

            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            personCommonProperties.SetBasicFullName();

            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;
        }
    }
}
