// <copyright file="ICustomerReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadModel.WithRelatedEntities;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Interface for customer and its related entities.
    /// </summary>
    public interface ICustomerReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the list of policy transactions.
        /// </summary>
        IEnumerable<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the list of policies.
        /// </summary>
        IEnumerable<PolicyReadModel> Policies { get; set; }

        /// <summary>
        /// Gets or sets the list of quote.
        /// </summary>
        IEnumerable<NewQuoteReadModel> Quotes { get; set; }

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
        /// Gets or sets the list of emails.
        /// </summary>
        IEnumerable<ReadWriteModel.Email.Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        UserReadModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the list of claims.
        /// </summary>
        IEnumerable<ClaimReadModel> Claims { get; set; }

        /// <summary>
        /// Gets or sets the list of documents related to the customer's quotes.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> QuoteDocuments { get; set; }

        /// <summary>
        /// Gets or sets the list of documents related to the customer's claims.
        /// </summary>
        IEnumerable<ClaimAttachmentReadModel> ClaimDocuments { get; set; }

        /// <summary>
        /// Gets or sets the primary person for this customer.
        /// </summary>
        PersonReadModel PrimaryPerson { get; set; }

        /// <summary>
        /// Gets or sets the street addresses of the people related to this customer.
        /// </summary>
        IEnumerable<StreetAddressReadModel> PersonStreetAddresses { get; set; }

        /// <summary>
        /// Gets or sets the email addresses of the people related to this customer.
        /// </summary>
        IEnumerable<EmailAddressReadModel> PersonEmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the phone numbers of the people related to the customer.
        /// </summary>
        IEnumerable<PhoneNumberReadModel> PersonPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the social media IDs of the people related to the customer.
        /// </summary>
        IEnumerable<SocialMediaIdReadModel> PersonSocialMediaIds { get; set; }

        /// <summary>
        /// Gets or sets the messenger IDs of the people related to the customer.
        /// </summary>
        IEnumerable<MessengerIdReadModel> PersonMessengerIds { get; set; }

        /// <summary>
        /// Gets or sets the addresses to the websites of the people related to the customer.
        /// </summary>
        IEnumerable<WebsiteAddressReadModel> PersonWebsiteAddreses { get; set; }

        /// <summary>
        /// Gets or sets the list of people related to the customer.
        /// </summary>
        IEnumerable<PersonReadModel> People { get; set; }

        /// <summary>
        /// Gets or sets the list of sms associated with the customer.
        /// </summary>
        IEnumerable<ReadWriteModel.Sms> Sms { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the customer.
        /// </summary>
        IEnumerable<Relationship> FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the customer.
        /// </summary>
        IEnumerable<Relationship> ToRelationships { get; set; }

        /// <summary>
        /// Gets or sets the portal that is associated with the customer.
        /// </summary>
        PortalReadModel Portal { get; set; }

        PortalLocations PortalLocations { get; set; }

        /// <summary>
        /// Gets or sets the list of saved payment methods associated with the customer.
        /// </summary>
        IEnumerable<SavedPaymentMethod> SavedPaymentMethods { get; set; }
    }
}
