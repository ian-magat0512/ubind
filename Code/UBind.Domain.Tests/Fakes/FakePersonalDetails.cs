// <copyright file="FakePersonalDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Helpers;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// For use in test data.
    /// </summary>
    public class FakePersonalDetails : IPersonalDetails
    {
        public FakePersonalDetails()
        {
            this.GenerateRepeatingFields();
        }

        public Guid TenantId { get; set; } = Guid.NewGuid();

        public Guid OrganisationId { get; set; }

        public DeploymentEnvironment? Environment { get; set; } = DeploymentEnvironment.Staging;

        public string FullName { get; set; } = "John Smith";

        public string PreferredName { get; set; } = "Jonno";

        public string Email { get; set; } = "john.smith@example.com";

        public string AlternativeEmail { get; set; } = "john.smith+alt@example.com";

        public string MobilePhone { get; set; } = "0412341236";

        public string HomePhone { get; set; } = "0412341234";

        public string WorkPhone { get; set; } = "0412341235";

        public string NamePrefix { get; set; } = "Dr";

        public string FirstName { get; set; } = "John";

        public string MiddleNames { get; set; } = "Doe";

        public string LastName { get; set; } = "Smith";

        public string NameSuffix { get; set; } = "Jr";

        public string Company { get; set; } = "uBind";

        public string Title { get; set; } = "Developer";

        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

        public List<StreetAddressField> StreetAddresses { get; set; }

        public List<PhoneNumberField> PhoneNumbers { get; set; }

        public List<MessengerIdField> MessengerIds { get; set; }

        public List<WebsiteAddressField> WebsiteAddresses { get; set; }

        public List<SocialMediaIdField> SocialMediaIds { get; set; }

        public List<EmailAddressField> EmailAddresses { get; set; }

        public string MobilePhoneNumber { get; set; }

        public string HomePhoneNumber { get; set; }

        public string WorkPhoneNumber { get; set; }

        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

        public string GetFullNameFromParts()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetAlternativeEmail(string email)
        {
            this.AlternativeEmail = email;
        }

        /// <inheritdoc/>
        public void SetEmail(string email)
        {
            this.Email = email;
        }

        /// <inheritdoc/>
        public void SetEmailIfNull()
        {
            PersonDetailExtension.SetEmailIfNull(this);
        }

        private void GenerateRepeatingFields()
        {
            this.EmailAddresses = new List<EmailAddressField>
                {
                    new EmailAddressField("home", string.Empty, new EmailAddress("test1@email.com")),
                    new EmailAddressField("work", string.Empty, new EmailAddress("test2@email.com")),
                };

            this.PhoneNumbers = new List<PhoneNumberField>
                {
                    new PhoneNumberField("home", string.Empty, new PhoneNumber("0412341234")),
                    new PhoneNumberField("work", string.Empty, new PhoneNumber("0412341235")),
                    new PhoneNumberField("mobile", string.Empty, new PhoneNumber("0412341236")),
                };

            this.StreetAddresses = new List<StreetAddressField>
                {
                    new StreetAddressField("Home", string.Empty, new Domain.ValueTypes.Address
                    { Line1 = "123", Postcode = "4000", State = Domain.ValueTypes.State.QLD, Suburb = "Sub" }),
                };
        }
    }
}
