// <copyright file="PersonReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class PersonReadModelSummary : EntityReadModel<Guid>, IPersonReadModelSummary
    {
        /// <summary>
        /// Gets or sets the Organistion Name.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <inheritdoc/>
        public string FullName { get; set; }

        /// <inheritdoc/>
        public string NamePrefix { get; set; }

        /// <inheritdoc/>
        public string FirstName { get; set; }

        /// <inheritdoc/>
        public string MiddleNames { get; set; }

        /// <inheritdoc/>
        public string LastName { get; set; }

        /// <inheritdoc/>
        public string NameSuffix { get; set; }

        /// <inheritdoc/>
        public string Company { get; set; }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string PreferredName { get; set; }

        /// <inheritdoc/>
        public string Email { get; set; }

        /// <inheritdoc/>
        public string AlternativeEmail { get; set; }

        /// <inheritdoc/>
        public string MobilePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string HomePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string WorkPhoneNumber { get; set; }

        /// <inheritdoc/>
        public string Status { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerId { get; set; }

        /// <inheritdoc/>
        public Guid? OwnerId { get; set; }

        /// <inheritdoc/>
        public string OwnerFullName { get; set; }

        /// <inheritdoc/>
        public bool HasActivePolicies { get; set; }

        /// <inheritdoc/>
        public Guid? UserId { get; set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

        /// <inheritdoc/>
        public string MobilePhone { get; set; }

        /// <inheritdoc/>
        public string HomePhone { get; set; }

        /// <inheritdoc/>
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
        public bool UserHasBeenInvitedToActivate { get; set; }

        /// <inheritdoc/>
        public bool UserHasBeenActivated { get; set; }

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
    }
}
