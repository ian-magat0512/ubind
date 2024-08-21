// <copyright file="IPersonalDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Interface for personal details.
    /// </summary>
    /// <remarks>
    /// This interface includes both data intrinsic to a person (i.e. there name and contact details)
    /// plus extra relational data, such as the tenant and organisation they belong to.
    /// We are keeping this interface since events persist some data using it.
    /// </remarks>
    public interface IPersonalDetails : IPersonData
    {
        /// <summary>
        /// Gets a person's tenant id.
        /// Note: JsonProperty when deserializing, this is should be backwards compatible to older data.
        /// </summary>
        [JsonProperty("TenantNewId")]
        Guid TenantId { get; }

        /// <summary>
        /// Gets the person's organisation Id.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets or sets a person's mobile phone number.
        /// </summary>
        string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a person's home phone number.
        /// </summary>
        string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a person's work phone number.
        /// </summary>
        string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's list of email addresses.
        /// </summary>
        [JsonProperty(PropertyName = "emailAddresses")]
        List<EmailAddressField> EmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the person's list of addresses.
        /// </summary>
        [JsonProperty(PropertyName = "streetAddresses")]
        List<StreetAddressField> StreetAddresses { get; set; }

        /// <summary>
        /// Gets or sets the person's list of phone numbers.
        /// </summary>
        [JsonProperty(PropertyName = "phoneNumbers")]
        List<PhoneNumberField> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the person's list of messengers.
        /// </summary>
        [JsonProperty(PropertyName = "messengerIds")]
        List<MessengerIdField> MessengerIds { get; set; }

        /// <summary>
        /// Gets or sets the person's list of websites.
        /// </summary>
        [JsonProperty(PropertyName = "websiteAddresses")]
        List<WebsiteAddressField> WebsiteAddresses { get; set; }

        /// <summary>
        /// Gets or sets the person's list of socials.
        /// </summary>
        [JsonProperty(PropertyName = "socialMediaIds")]
        List<SocialMediaIdField> SocialMediaIds { get; set; }

        /// <summary>
        /// Sets the email if its null, gets the first email on the email addresses
        /// </summary>
        void SetEmailIfNull();
    }
}
