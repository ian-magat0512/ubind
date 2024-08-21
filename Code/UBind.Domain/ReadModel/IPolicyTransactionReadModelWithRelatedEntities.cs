// <copyright file="IPolicyTransactionReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadModel.WithRelatedEntities;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Interface for policy transaction and its related entities.
    /// </summary>
    public interface IPolicyTransactionReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
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
        /// Gets or sets the list of policy transactions related to the policy.
        /// </summary>
        /// <remarks>This is only populated if policy is included.</remarks>
        IEnumerable<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        NewQuoteReadModel Quote { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        Domain.Product.Product Product { get; set; }

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
        /// Gets or sets the organisation associated with the transaction.
        /// </summary>
        OrganisationReadModel Organisation { get; set; }

        /// <summary>
        /// Gets or sets the emails associated with a transaction.
        /// </summary>
        IEnumerable<ReadWriteModel.Email.Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the owner of the transaction.
        /// </summary>
        UserReadModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the customer of the transaction.
        /// </summary>
        CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the documents associated with a transaction.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets or sets the list sms associated with the transaction.
        /// </summary>
        IEnumerable<ReadWriteModel.Sms> Sms { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the transaction.
        /// </summary>
        IEnumerable<Relationship> FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the transaction.
        /// </summary>
        IEnumerable<Relationship> ToRelationships { get; set; }

        /// <summary>
        /// Gets the time zone applicable to dates in the policy transaction.
        /// </summary>
        DateTimeZone TimeZone { get; }
    }
}
