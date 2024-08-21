// <copyright file="PersonReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Person.Fields;

    /// <summary>
    /// Read model for Persons.
    /// </summary>
    public class PersonReadModel : PersonCommonProperties
    {
        // For person read model related entities
        public PersonReadModel(Guid id)
        {
            this.Id = id;
        }

        // Parameterless constructor for EF.
        private PersonReadModel()
        {
        }

        private PersonReadModel(
            Guid tenantId,
            Guid organisationId,
            Guid id,
            string fullName,
            string preferredName,
            string email,
            string alternativeEmail,
            string mobilePhoneNumber,
            string homePhoneNumber,
            string workPhoneNumber,
            Instant createdTimestamp,
            bool isTestData)
        {
            this.Id = id;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.FullName = fullName;
            this.PreferredName = preferredName;

            this.SetNameComponentsFromFullNameIfNoneAlreadySet();
            this.SetBasicFullName();

            this.Email = email;
            this.AlternativeEmail = alternativeEmail;
            this.MobilePhoneNumber = mobilePhoneNumber;
            this.HomePhoneNumber = homePhoneNumber;
            this.WorkPhoneNumber = workPhoneNumber;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;
            this.IsTestData = isTestData;
        }

        private PersonReadModel(
            Guid tenantId, Guid organisationId, Guid personId, Instant createdTimestamp, bool isTestData = false)
        {
            this.Id = personId;
            this.TenantId = tenantId;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = this.CreatedTimestamp;
            this.OrganisationId = organisationId;
            this.IsTestData = isTestData;
        }

        private PersonReadModel(
            Guid tenantId,
            Guid id,
            Guid? customerId,
            Guid? userId,
            PersonData personData,
            Instant createdTimestamp,
            bool isTestData)
        {
            this.Id = id;
            this.CustomerId = customerId;
            this.UserId = userId;
            this.TenantId = tenantId;
            this.OrganisationId = personData.OrganisationId;
            this.FullName = personData.FullName;
            this.PreferredName = personData.PreferredName;
            this.NamePrefix = personData.NamePrefix;
            this.FirstName = personData.FirstName;
            this.MiddleNames = personData.MiddleNames;
            this.LastName = personData.LastName;
            this.NameSuffix = personData.NameSuffix;

            this.SetNameComponentsFromFullNameIfNoneAlreadySet();
            this.SetBasicFullName();

            this.Email = personData.Email;
            this.AlternativeEmail = personData.AlternativeEmail;
            this.MobilePhoneNumber = personData.MobilePhone;
            this.HomePhoneNumber = personData.HomePhone;
            this.WorkPhoneNumber = personData.WorkPhone;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = this.CreatedTimestamp;
            this.Company = personData.Company;
            this.Title = personData.Title;
            this.IsTestData = isTestData;

            this.SetRepeatingFieldsFromValueObjects(personData);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user the person relates to, if any, otherwise default.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the customer ID the person relates to.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a user for this person and
        /// the user has been invited to activate.
        /// </summary>
        public bool UserHasBeenInvitedToActivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a user for this person and
        /// the user has been activated.
        /// </summary>
        public bool UserHasBeenActivated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a user for this person and
        /// the user is blocked.
        /// </summary>
        public bool UserIsBlocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets the email addresses.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<EmailAddressReadModel> EmailAddresses { get; private set; } = new Collection<EmailAddressReadModel>();

        /// <summary>
        /// Gets the phone numbers.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<PhoneNumberReadModel> PhoneNumbers { get; private set; } = new Collection<PhoneNumberReadModel>();

        /// <summary>
        /// Gets the street addresses.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<StreetAddressReadModel> StreetAddresses { get; private set; } = new Collection<StreetAddressReadModel>();

        /// <summary>
        /// Gets the web addresses.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<WebsiteAddressReadModel> WebsiteAddresses { get; private set; } = new Collection<WebsiteAddressReadModel>();

        /// <summary>
        /// Gets the messenger IDs.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<MessengerIdReadModel> MessengerIds { get; private set; } = new Collection<MessengerIdReadModel>();

        /// <summary>
        /// Gets the social media IDs.
        /// </summary>
        /// <remarks>
        /// Only here for EF to generate the join table for one-to-many relationship.
        /// .</remarks>
        public virtual ICollection<SocialMediaIdReadModel> SocialMediaIds { get; private set; } = new Collection<SocialMediaIdReadModel>();

        /// <summary>
        /// Factory method for creating a person read model from imported event.
        /// </summary>
        /// <param name="id">The ID of the person.</param>
        /// <param name="tenantId">The ID of the tenant the person belong to.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="fullName">The full name of the person.</param>
        /// <param name="preferredName">The preferred name of the person.</param>
        /// <param name="email">The primary email of the person.</param>
        /// <param name="alternativeEmail">The alternative email of the person.</param>
        /// <param name="mobilePhoneNumber">The mobile phone number of the person.</param>
        /// <param name="homePhoneNumber">The home phone number of the person.</param>
        /// <param name="workPhoneNumber">The work phone number of the person.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        /// <returns>A new instance of <see cref="PersonReadModel"/>.</returns>
        public static PersonReadModel CreateImportedPerson(
            Guid tenantId,
            Guid organisationId,
            Guid id,
            string fullName,
            string preferredName,
            string email,
            string alternativeEmail,
            string mobilePhoneNumber,
            string homePhoneNumber,
            string workPhoneNumber,
            Instant createdTimestamp,
            bool isTestData)
        {
            return new PersonReadModel(
                tenantId,
                organisationId,
                id,
                fullName,
                preferredName,
                email,
                alternativeEmail,
                mobilePhoneNumber,
                homePhoneNumber,
                workPhoneNumber,
                createdTimestamp,
                isTestData);
        }

        /// <summary>
        /// Factory method for creating a simple person read model.
        /// </summary>
        /// <param name="id">The ID of the person.</param>
        /// <param name="tenantId">The ID of the tenant the person belong to.</param>
        /// <param name="createdTimestamp">The time the person was created.</param>
        /// <param name="organisationId">The ID of the organisation the person belong to.</param>
        /// <returns>A new instance of <see cref="PersonReadModel"/>.</returns>
        public static PersonReadModel CreatePerson(
            Guid tenantId, Guid organisationId, Guid id, Instant createdTimestamp, bool isTestData = false)
        {
            return new PersonReadModel(tenantId, organisationId, id, createdTimestamp, isTestData);
        }

        /// <summary>
        /// Factory method for creating a person read model from person data.
        /// </summary>
        /// <param name="id">The ID of the person.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="personData">The data of the person.</param>
        /// <param name="createdTimestamp">The time the person was created.</param>
        /// <returns>A new instance of <see cref="PersonReadModel"/>.</returns>
        public static PersonReadModel CreateFromPersonData(
            Guid tenantId,
            Guid id,
            Guid? customerId,
            Guid? userId,
            PersonData personData,
            Instant createdTimestamp,
            bool isTestData = false)
        {
            return new PersonReadModel(tenantId, id, customerId, userId, personData, createdTimestamp, isTestData);
        }

        private void SetRepeatingFieldsFromValueObjects(PersonData personData)
        {
            if (personData.EmailAddresses != null)
            {
                if (personData.EmailAddresses.Any())
                {
                    var emailAddresses = personData.EmailAddresses
                        .Select(s => new EmailAddressReadModel(personData.TenantId, s));
                    foreach (var emailAddress in emailAddresses)
                    {
                        this.EmailAddresses.Add(emailAddress);
                    }
                }
            }

            if (personData.PhoneNumbers != null)
            {
                if (personData.PhoneNumbers.Any())
                {
                    var phoneNumbers = personData.PhoneNumbers
                        .Select(s => new PhoneNumberReadModel(personData.TenantId, s));
                    foreach (var phoneNumber in phoneNumbers)
                    {
                        this.PhoneNumbers.Add(phoneNumber);
                    }
                }
            }

            if (personData.StreetAddresses != null)
            {
                if (personData.StreetAddresses.Any())
                {
                    var addresses = personData.StreetAddresses
                        .Select(s => new StreetAddressReadModel(personData.TenantId, s));
                    foreach (var address in addresses)
                    {
                        this.StreetAddresses.Add(address);
                    }
                }
            }

            if (personData.MessengerIds != null)
            {
                if (personData.MessengerIds.Any())
                {
                    var messengers = personData.MessengerIds
                        .Select(s => new MessengerIdReadModel(personData.TenantId, s));
                    foreach (var messenger in messengers)
                    {
                        this.MessengerIds.Add(messenger);
                    }
                }
            }

            if (personData.SocialMediaIds != null)
            {
                if (personData.SocialMediaIds.Any())
                {
                    var socials = personData.SocialMediaIds
                        .Select(s => new SocialMediaIdReadModel(personData.TenantId, s));
                    foreach (var social in socials)
                    {
                        this.SocialMediaIds.Add(social);
                    }
                }
            }

            if (personData.WebsiteAddresses != null)
            {
                if (personData.WebsiteAddresses.Any())
                {
                    var websites = personData.WebsiteAddresses
                        .Select(s => new WebsiteAddressReadModel(personData.TenantId, s));
                    foreach (var website in websites)
                    {
                        this.WebsiteAddresses.Add(website);
                    }
                }
            }
        }
    }
}
