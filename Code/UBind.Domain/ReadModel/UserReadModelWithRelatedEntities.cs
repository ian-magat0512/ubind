// <copyright file="UserReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// This class is needed because we need a data transfer object for user and its related entities.
    /// </summary>
    public class UserReadModelWithRelatedEntities : IUserReadModelWithRelatedEntities
    {
        /// <inheritdoc/>
        public Tenant Tenant { get; set; }

        /// <inheritdoc/>
        public UserReadModel User { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <inheritdoc/>
        public OrganisationReadModel Organisation { get; set; }

        /// <inheritdoc/>
        public PersonReadModel Person { get; set; }

        /// <inheritdoc/>
        public IEnumerable<StreetAddressReadModel> StreetAddresses { get; set; }

        /// <inheritdoc/>
        public IEnumerable<EmailAddressReadModel> EmailAddresses { get; set; }

        /// <inheritdoc/>
        public IEnumerable<PhoneNumberReadModel> PhoneNumbers { get; set; }

        /// <inheritdoc/>
        public IEnumerable<MessengerIdReadModel> MessengerIds { get; set; }

        /// <inheritdoc/>
        public IEnumerable<SocialMediaIdReadModel> SocialMediaIds { get; set; }

        /// <inheritdoc/>
        public IEnumerable<WebsiteAddressReadModel> WebsiteAddresses { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Role> Roles { get; set; }

        /// <inheritdoc/>
        public PortalReadModel Portal { get; set; }

        public PortalLocations PortalLocations { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TextAdditionalPropertyValueReadModel> TextAdditionalPropertiesValues { get; set; }

        /// <inheritdoc/>
        public IEnumerable<StructuredDataAdditionalPropertyValueReadModel> StructuredDataAdditionalPropertyValues { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IAdditionalPropertyValueReadModel> AdditionalPropertyValues
        {
            get
            {
                return this.TextAdditionalPropertiesValues.Cast<IAdditionalPropertyValueReadModel>()
                    .Concat(this.StructuredDataAdditionalPropertyValues);
            }
        }
    }
}
