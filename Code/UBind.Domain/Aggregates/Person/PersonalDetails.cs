// <copyright file="PersonalDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Encapsulates person data for use in events.
    /// </summary>
    public class PersonalDetails : IPersonalDetails
    {
        private readonly Regex nameRegex = new Regex("^[A-Za-z][A-Za-z\\s\\-\\',.]*$");
        private readonly Regex companyRegex = new Regex("^([a-zA-Z '?!&+%:$-@\\/\\\\\\\\\\(\\),]*)+$");
        private readonly Regex customLabel = new Regex("^[a-zA-Z0-9\\-\\s]+$");
        private readonly Regex webAddressRegex = new Regex(@"^(?:(?:(?:https?|ftp):)?\/\/)?(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?");

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalDetails"/> class.
        /// </summary>
        /// <param name="source">An object implementing <see cref="IPersonalDetails"/> to take the data from.</param>
        public PersonalDetails(IPersonalDetails source)
        {
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = source.FullName;
            personCommonProperties.PreferredName = source.PreferredName;
            personCommonProperties.NamePrefix = source.NamePrefix;
            personCommonProperties.FirstName = source.FirstName;
            personCommonProperties.MiddleNames = source.MiddleNames;
            personCommonProperties.LastName = source.LastName;
            personCommonProperties.NameSuffix = source.NameSuffix;
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            personCommonProperties.SetBasicFullName();

            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;

            this.TenantId = source.TenantId;
            this.Company = source.Company;
            this.Title = source.Title;
            this.Email = source.Email;
            this.AlternativeEmail = source.AlternativeEmail;
            this.MobilePhone = source.MobilePhone;
            this.HomePhone = source.HomePhone;
            this.WorkPhone = source.WorkPhone;
            this.OrganisationId = source.OrganisationId;

            this.EmailAddresses = source.EmailAddresses?.ToList() ?? new List<EmailAddressField>();
            this.PhoneNumbers = source.PhoneNumbers?.ToList() ?? new List<PhoneNumberField>();
            this.StreetAddresses = source.StreetAddresses?.ToList() ?? new List<StreetAddressField>();
            this.WebsiteAddresses = source.WebsiteAddresses?.ToList() ?? new List<WebsiteAddressField>();
            this.MessengerIds = source.MessengerIds?.ToList() ?? new List<MessengerIdField>();
            this.SocialMediaIds = source.SocialMediaIds?.ToList() ?? new List<SocialMediaIdField>();
            this.EmailAddresses = source.EmailAddresses ?? new List<EmailAddressField>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalDetails"/> class.
        /// </summary>
        /// <param name="personCommonProperties">An instance of PersonCommonProperties.</param>
        public PersonalDetails(Guid tenantId, PersonCommonProperties personCommonProperties)
        {
            this.TenantId = tenantId;
            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;
            this.Company = personCommonProperties.Company;
            this.Title = personCommonProperties.Title;
            this.Email = personCommonProperties.Email;
            this.AlternativeEmail = personCommonProperties.AlternativeEmail;
            this.MobilePhone = this.MobilePhoneNumber = personCommonProperties.MobilePhoneNumber;
            this.HomePhone = this.HomePhoneNumber = personCommonProperties.HomePhoneNumber;
            this.WorkPhone = this.WorkPhoneNumber = personCommonProperties.WorkPhoneNumber;
            this.OrganisationId = personCommonProperties.OrganisationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalDetails"/> class.
        /// </summary>
        [JsonConstructor]
        public PersonalDetails()
        {
        }

        /// <summary>
        /// Gets or sets the person's tenant.
        /// Note: JsonProperty when deserializing, this is should be backwards compatible to older data.
        /// </summary>
        [JsonProperty("TenantNewId")]
        public virtual Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the person's organisation.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the person's customer ID.
        /// </summary>
        [JsonProperty]
        [Obsolete("This is not used anywhere else, not even set or used.")]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        [JsonProperty]
        public string FullName { get; set; }

        /// <summary>
        /// Gets a person's first name and last name.
        /// </summary>
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

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
        /// Gets or sets the user's title.
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the person's preferred name.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the person's primary email address.
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's login email address.
        /// </summary>
        [JsonProperty]
        public string LoginEmail { get; set; }

        /// <summary>
        /// Gets or sets the person's alternate email address.
        /// </summary>
        [JsonProperty]
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the person's mobile phone.
        /// </summary>
        [JsonProperty]
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the person's home phone.
        /// </summary>
        [JsonProperty]
        public string HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the person's work phone.
        /// </summary>
        [JsonProperty]
        public string WorkPhone { get; set; }

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

        public void Validate()
        {
            this.ValidateName("FirstName", this.FirstName);
            this.ValidateName("LastName", this?.LastName);
            this.ValidateName("MiddleName", this?.MiddleNames);
            this.ValidateName("NamePrefix", this?.NamePrefix);
            this.ValidateName("NameSuffix", this?.NameSuffix);
            this.ValidateName("PreferredName", this?.PreferredName);
            this.ValidateName("Title", this?.Title);
            this.ValidateCompany(this?.Company);
            this.ValidatePhoneNumbers();
            this.ValidateEmailAddresses();
            this.ValidateStreetAddresses();
            this.ValidatetWebsiteAddresses();
            this.ValidateMessengerIds();
            this.ValidateSocialmediaIds();
        }

        public void ValidatePhoneNumbers()
        {
            if (this.PhoneNumbers == null || !this.PhoneNumbers.Any())
            {
                return;
            }

            this.ValidateAndSetDefaultValue(this.PhoneNumbers?.ToList());

            foreach (var phoneNumber in this.PhoneNumbers)
            {
                if (!phoneNumber.IsValid())
                {
                    var error = Errors.Person.InvalidField(phoneNumber, phoneNumber.PhoneNumber);
                    throw new ErrorException(error);
                }

                this.ValidateLabel("phone number", phoneNumber.PhoneNumber, phoneNumber);
            }
        }

        public void ValidateEmailAddresses()
        {
            if (this.EmailAddresses == null || !this.EmailAddresses.Any())
            {
                return;
            }

            this.ValidateAndSetDefaultValue(this.EmailAddresses?.ToList());
            foreach (var emailAddress in this.EmailAddresses)
            {
                if (!emailAddress.IsValid())
                {
                    var error = Errors.Person.InvalidField(emailAddress, emailAddress.EmailAddress);
                    throw new ErrorException(error);
                }

                this.ValidateLabel("email address", emailAddress.EmailAddress, emailAddress);
            }
        }

        public void ValidateStreetAddresses()
        {
            if (this.StreetAddresses == null || !this.StreetAddresses.Any())
            {
                return;
            }

            IPostcodeValidator postcodeValidator = PostcodeValidatorFactory.GetPostcodeValidatorByCountryCode("AU");

            this.ValidateAndSetDefaultValue(this.StreetAddresses?.ToList());
            foreach (var x in this.StreetAddresses)
            {
                this.ValidateLabel("street address", x.Address + " " + x.Suburb + " " + x.State + " " + x.Postcode, x);

                var hasEmpty = string.IsNullOrEmpty(x.Address) || string.IsNullOrEmpty(x.Postcode) || string.IsNullOrEmpty(x.State) || string.IsNullOrEmpty(x.Suburb);
                if (hasEmpty)
                {
                    var error = Errors.Person.StreetAddressIncomplete(x.Address, x.Suburb, x.State, x.Postcode);
                    throw new ErrorException(error);
                }

                if (!postcodeValidator.IsValidState(x.State))
                {
                    var states = postcodeValidator.GetValidStates();
                    var error = Errors.Person.StreetAddressStateInvalid(this.GetLabel(x), x.State, states);
                    throw new ErrorException(error);
                }

                if (!postcodeValidator.IsValidPostCode(x.Postcode) ||
                    (postcodeValidator.IsValidPostCode(x.Postcode)
                        && !postcodeValidator.IsValidPostCodeForTheState(x.State, x.Postcode)))
                {
                    var error = Errors.Person.StreetAddressStateAndPostcodeMismatch(this.GetLabel(x), x.State, x.Postcode);
                    throw new ErrorException(error);
                }
            }
        }

        public void ValidatetWebsiteAddresses()
        {
            if (this?.WebsiteAddresses == null || !this.WebsiteAddresses.Any())
            {
                return;
            }

            this.ValidateAndSetDefaultValue(this.WebsiteAddresses?.ToList());

            foreach (var websiteAddress in this.WebsiteAddresses)
            {
                var match = this.webAddressRegex.IsMatch(websiteAddress.WebsiteAddress);
                if (!match)
                {
                    var error = Errors.Person.InvalidField(websiteAddress, websiteAddress.WebsiteAddress);
                    throw new ErrorException(error);
                }

                this.ValidateLabel("website address", websiteAddress.WebsiteAddress, websiteAddress);
            }
        }

        public void ValidateMessengerIds()
        {
            if (this?.MessengerIds == null || !this.MessengerIds.Any())
            {
                return;
            }

            this.ValidateAndSetDefaultValue(this.MessengerIds?.ToList());

            foreach (var messengerId in this.MessengerIds)
            {
                if (!string.IsNullOrEmpty(messengerId.MessengerId))
                {
                    this.ValidateLabel("messenger ID", messengerId.MessengerId, messengerId);
                }
            }
        }

        public void ValidateSocialmediaIds()
        {
            if (this?.SocialMediaIds == null || !this.SocialMediaIds.Any())
            {
                return;
            }

            this.ValidateAndSetDefaultValue(this.SocialMediaIds?.ToList());

            foreach (var socialMedia in this.SocialMediaIds)
            {
                if (!string.IsNullOrEmpty(socialMedia.SocialMediaId))
                {
                    this.ValidateLabel("social media ID", socialMedia.SocialMediaId, socialMedia);
                }
            }
        }

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

        private void ValidateLabel<T>(string context, string value, T item)
            where T : LabelledOrderedField
        {
            var match = this.customLabel.IsMatch(item.Label);

            if (!match)
            {
                var error = Errors.Person.CustomLabelInvalid(this.GetLabel(item), context, value);
                throw new ErrorException(error);
            }
        }

        private void ValidateAndSetDefaultValue<T>(List<T> items)
            where T : LabelledOrderedField
        {
            if (items == null || !items.Any())
            {
                return;
            }

            var moreThanOneDefault = items.Count(x => x.IsDefault) > 1;

            if (moreThanOneDefault)
            {
                var contextValue = items[0].GetType().Name
                    .Humanize()
                    .ToLower()
                    .Replace("id", "ID")
                    .Replace("field", string.Empty)
                    .Trim();
                var error = Errors.Person.MoreThanOneItemHasDefault(contextValue);
                throw new ErrorException(error);
            }

            var noDefaultSelected = items.All(x => !x.IsDefault);
            if (noDefaultSelected && items.Any())
            {
                items[0].IsDefault = true;
            }
        }

        private void ValidateCompany(string company)
        {
            if (!string.IsNullOrEmpty(company) && !this.companyRegex.IsMatch(company))
            {
                var error = Errors.Person.CompanyNameInvalid(company);
                throw new ErrorException(error);
            }
        }

        private void ValidateName(string nameType, string name)
        {
            if (!string.IsNullOrEmpty(name) && !this.nameRegex.IsMatch(name))
            {
                var error = Errors.Person.NameInvalid(nameType, name);
                throw new ErrorException(error);
            }
        }

        /// <summary>
        /// This is to distinguish custom label from regular label, since label with value "other" will have a custom label.
        /// </summary>
        private string GetLabel<T>(T item)
            where T : LabelledOrderedField
        {
            return item.Label.ToLower() != "other" ? item.Label.Humanize() : item.CustomLabel;
        }
    }
}
