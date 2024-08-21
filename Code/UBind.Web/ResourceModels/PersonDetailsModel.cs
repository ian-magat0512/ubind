// <copyright file="PersonDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Validation;

    /// <summary>
    /// For representing Person model.
    /// </summary>
    public class PersonDetailsModel
    {
        public PersonDetailsModel(IPersonReadModelSummary person)
        {
            if (person == null)
            {
                return;
            }

            this.TenantId = ((IEntityReadModel<Guid>)person).TenantId;
            this.Id = person.Id;
            this.OrganisationId = person.OrganisationId;
            this.FullName = person.FullName;
            this.FirstName = person.FirstName;
            this.LastName = person.LastName;
            this.NamePrefix = person.NamePrefix;
            this.NameSuffix = person.NameSuffix;
            this.MiddleNames = person.MiddleNames;
            this.PreferredName = person.PreferredName;
            this.Company = person.Company;
            this.Title = person.Title;
            this.Email = person.Email;
            this.AlternativeEmail = person.AlternativeEmail;
            this.HomePhoneNumber = person.HomePhoneNumber;
            this.WorkPhoneNumber = person.WorkPhoneNumber;
            this.MobilePhoneNumber = person.MobilePhoneNumber;
            this.Status = person.Status;
            this.OwnerId = person.OwnerId;
            this.OwnerFullName = person.OwnerFullName;
            this.HasActivePolicies = person.HasActivePolicies;
            this.CreatedDateTime = person.CreatedTimestamp.ToExtendedIso8601String();
            this.CustomerId = person.CustomerId.GetValueOrDefault();
            this.LastModifiedDateTime = person.LastModifiedTimestamp.ToExtendedIso8601String();
            this.DisplayName = PersonPropertyHelper.GetDisplayName(person);
            this.UserId = person.UserId;

            this.SetRepeatingFieldsValues(person);
            this.CheckCreateRepeatingFieldsForBackwardCompatibility();
            this.CheckAndFixRepeatingFieldsDefaults();
        }

        [JsonConstructor]
        public PersonDetailsModel()
        {
        }

        /// <summary>
        /// Gets or sets the person's ID.
        /// </summary>
        [JsonProperty]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the person's customerId.
        /// </summary>
        [JsonProperty]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        [JsonProperty]
        [Name]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the person's preferred name.
        /// </summary>
        [JsonProperty]
        [Name]
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the person email address.
        /// </summary>
        [JsonProperty]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's alternative email address.
        /// </summary>
        [JsonProperty]
        [OptionalEmailAddress]
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the person's mobile phone number.
        /// </summary>
        [JsonProperty]
        [AustralianMobileNumber(ErrorMessage = "Mobile phone number must be in australian format.")]
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's home phone number.
        /// </summary>
        [JsonProperty]
        [AustralianPhoneNumber(ErrorMessage = "Home phone must be in australian format.")]
        public string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's work phone number.
        /// </summary>
        [JsonProperty]
        [AustralianPhoneNumber(ErrorMessage = "Work phone must be in australian format.")]
        public string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's login email address.
        /// </summary>
        [JsonProperty]
        public string LoginEmail { get; set; }

        /// <summary>
        /// Gets or sets the person's name prefix.
        /// </summary>
        [JsonProperty]
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        [JsonProperty]
        [Name]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's middle names.
        /// </summary>
        [JsonProperty]
        [Name]
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        [JsonProperty]
        [Name]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        [JsonProperty]
        [Name]
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the person's company.
        /// </summary>
        [JsonProperty]
        [Name]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the person's title.
        /// </summary>
        [JsonProperty]
        [Name]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user who owns this customer.
        /// </summary>
        [JsonProperty]
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the id of the user who owns this person.
        /// </summary>
        [JsonProperty]
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the created times.
        /// </summary>
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modifed time.
        /// </summary>
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets a value indicating whether a person has active policy.
        /// </summary>
        [JsonProperty]
        public bool HasActivePolicies { get; private set; }

        /// <summary>
        /// Gets or sets the customer's status.
        /// </summary>
        [JsonProperty(PropertyName = "userStatus")]
        public string Status { get; set; }

        [JsonProperty]
        public Guid TenantId { get; set; }

        [JsonProperty]
        public Guid? OrganisationId { get; set; }

        [JsonProperty]
        public string OrganisationAlias { get; set; }

        [JsonProperty]
        public string DisplayName { get; private set; }

        [JsonProperty]
        public string MobilePhone { get; set; }

        [JsonProperty]
        public string HomePhone { get; set; }

        [JsonProperty]
        public string WorkPhone { get; set; }

        [JsonProperty]
        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        [JsonProperty]
        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        [JsonProperty]
        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        [JsonProperty]
        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        [JsonProperty]
        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        [JsonProperty]
        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

        /// <summary>
        /// Gets or sets the id of the user who is linked to this person.
        /// </summary>
        [JsonProperty]
        public Guid? UserId { get; set; }

        public void SetRepeatingFieldsValues(IPersonReadModelSummary person = null)
        {
            if (person == null)
            {
                return;
            }

            if (person.EmailAddresses != null)
            {
                this.EmailAddresses = person.EmailAddresses.ToList();
            }

            if (person.PhoneNumbers != null)
            {
                this.PhoneNumbers = person.PhoneNumbers.ToList();
            }

            if (person.StreetAddresses != null)
            {
                this.StreetAddresses = person.StreetAddresses.ToList();
            }

            if (person.WebsiteAddresses != null)
            {
                this.WebsiteAddresses = person.WebsiteAddresses.ToList();
            }

            if (person.MessengerIds != null)
            {
                this.MessengerIds = person.MessengerIds.ToList();
            }

            if (person.SocialMediaIds != null)
            {
                this.SocialMediaIds = person.SocialMediaIds.ToList();
            }
        }

        public void CheckCreateRepeatingFieldsForBackwardCompatibility()
        {
            if (this.EmailAddresses == null)
            {
                this.EmailAddresses = new List<EmailAddressField>();
            }

            if (this.PhoneNumbers == null)
            {
                this.PhoneNumbers = new List<PhoneNumberField>();
            }

            // check old phone fields
            if (!this.PhoneNumbers.Any())
            {
                if (!string.IsNullOrEmpty(this.HomePhoneNumber))
                {
                    var homePhone = new PhoneNumberField("home", string.Empty, new PhoneNumber(this.HomePhoneNumber));
                    this.PhoneNumbers.Add(homePhone);
                }

                if (!string.IsNullOrEmpty(this.WorkPhoneNumber))
                {
                    var workPhone = new PhoneNumberField("work", string.Empty, new PhoneNumber(this.WorkPhoneNumber));
                    this.PhoneNumbers.Add(workPhone);
                }

                if (!string.IsNullOrEmpty(this.MobilePhoneNumber))
                {
                    var mobilePhone = new PhoneNumberField("mobile", string.Empty, new PhoneNumber(this.MobilePhoneNumber));
                    this.PhoneNumbers.Add(mobilePhone);
                }
            }
        }

        public void CheckAndFixRepeatingFieldsDefaults()
        {
            this.SetOneFieldToDefaultIfNone(this.EmailAddresses);
            this.SetOneFieldToDefaultIfNone(this.PhoneNumbers);
            this.SetOneFieldToDefaultIfNone(this.StreetAddresses);
            this.SetOneFieldToDefaultIfNone(this.WebsiteAddresses);
            this.SetOneFieldToDefaultIfNone(this.MessengerIds);
            this.SetOneFieldToDefaultIfNone(this.SocialMediaIds);
        }

        private void SetOneFieldToDefaultIfNone<T>(List<T> fields)
            where T : LabelledOrderedField
        {
            if (fields != null && fields.Any() && !fields.Any(e => e.IsDefault))
            {
                fields.FirstOrDefault().IsDefault = true;
            }
        }
    }
}
