// <copyright file="IQuoteReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadModel.WithRelatedEntities;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Interface for quote and its related entities.
    /// </summary>
    public interface IQuoteReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the policy transaction.
        /// </summary>
        PolicyTransaction PolicyTransaction { get; set; }

        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        PolicyReadModel Policy { get; set; }

        /// <summary>
        /// Gets or sets the policy transactions related to the policy of this quote.
        /// </summary>
        /// <remarks>This property is only populated if policy is available.</remarks>
        IEnumerable<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        NewQuoteReadModel Quote { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        Domain.Product.Product Product { get; set; }

        ReleaseBase ProductRelease { get; set; }

        /// <summary>
        /// Gets or sets the list of product details.
        /// </summary>
        IEnumerable<ProductDetails> ProductDetails { get; set; }

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
        /// Gets or sets the emails associated with a quote.
        /// </summary>
        IEnumerable<ReadWriteModel.Email.Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the quote versions associated with a quote.
        /// </summary>
        IEnumerable<QuoteVersionReadModel> QuoteVersions { get; set; }

        /// <summary>
        /// Gets or sets the owner of the quote.
        /// </summary>
        UserReadModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the customer of the quote.
        /// </summary>
        CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the documents associated with a quote.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets or sets the list of sms associated with the quote.
        /// </summary>
        IEnumerable<ReadWriteModel.Sms> Sms { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the quote.
        /// </summary>
        IEnumerable<Relationship> FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the quote.
        /// </summary>
        IEnumerable<Relationship> ToRelationships { get; set; }
    }
}
