// <copyright file="UserUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Resource model for handling user updates.
    /// </summary>
    public class UserUpdateModel : PersonalDetails
    {
        /// <summary>
        /// Gets or sets the tenant ID or alias which the user belongs to.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the picture of the user represented as a byte array.
        /// </summary>
        public byte[] PictureData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is blocked or not.
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal the user should log into.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Converts the user update data to its equivalent user update model representation.
        /// </summary>
        /// <returns>The user update model.</returns>
        public Application.User.UserUpdateModel ToUserUpdateModel()
        {
            var model = new Application.User.UserUpdateModel();
            model.Email = this.Email;
            model.Blocked = this.Blocked;
            model.PreferredName = this.PreferredName;
            model.FullName = this.FullName;
            model.NamePrefix = this.NamePrefix;
            model.FirstName = this.FirstName;
            model.MiddleNames = this.MiddleNames;
            model.LastName = this.LastName;
            model.NameSuffix = this.NameSuffix;
            model.Company = this.Company;
            model.Title = this.Title;
            model.Email = this.Email;
            model.AlternativeEmail = this.AlternativeEmail;
            model.PictureData = this.PictureData;
            model.Blocked = this.Blocked;
            model.EmailAddresses = this.EmailAddresses;
            model.PhoneNumbers = this.PhoneNumbers;
            model.StreetAddresses = this.StreetAddresses;
            model.WebsiteAddresses = this.WebsiteAddresses;
            model.MessengerIds = this.MessengerIds;
            model.SocialMediaIds = this.SocialMediaIds;
            model.TenantId = this.TenantId;
            model.TenantId = this.TenantId;
            model.OrganisationId = this.OrganisationId;
            model.PortalId = this.PortalId;
            return model;
        }
    }
}
