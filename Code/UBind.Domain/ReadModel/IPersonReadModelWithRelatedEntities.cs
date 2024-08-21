// <copyright file="IPersonReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// The interface for the person data model with its related entities.
    /// </summary>
    public interface IPersonReadModelWithRelatedEntities : IEntityWithRelatedEntities
    {
        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the list of tenant details.
        /// </summary>
        IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <summary>
        /// Gets or sets the organisation.
        /// </summary>
        OrganisationReadModel Organisation { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        PersonReadModel Person { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        UserReadModel User { get; set; }

        /// <summary>
        /// Gets or sets the list of email addresses.
        /// </summary>
        IEnumerable<EmailAddressReadModel> EmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of phone numbers.
        /// </summary>
        IEnumerable<PhoneNumberReadModel> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the list of street addresses.
        /// </summary>
        IEnumerable<StreetAddressReadModel> StreetAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of website addresses.
        /// </summary>
        IEnumerable<WebsiteAddressReadModel> WebsiteAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of messenger IDs.
        /// </summary>
        IEnumerable<MessengerIdReadModel> MessengerIds { get; set; }

        /// <summary>
        /// Gets or sets the list of social media IDs.
        /// </summary>
        IEnumerable<SocialMediaIdReadModel> SocialMediaIds { get; set; }
    }
}
