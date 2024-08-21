// <copyright file="IPolicyReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadModel.WithRelatedEntities;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Interface for policy and its related entities.
    /// </summary>
    public interface IPolicyReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the list of policy transactions.
        /// </summary>
        IEnumerable<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        PolicyReadModel Policy { get; set; }

        /// <summary>
        /// Gets or sets the quotes.
        /// </summary>
        IEnumerable<NewQuoteReadModel> Quotes { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        IEnumerable<ClaimReadModel> Claims { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        UBind.Domain.Product.Product Product { get; set; }

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
        /// Gets or sets the organisation associated with the policy.
        /// </summary>
        OrganisationReadModel Organisation { get; set; }

        /// <summary>
        /// Gets or sets the emails associated with a policy.
        /// </summary>
        IEnumerable<ReadWriteModel.Email.Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the owner of the policy.
        /// </summary>
        UserReadModel Owner { get; set; }

        /// <summary>
        /// Gets or sets the customer of the policy.
        /// </summary>
        CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the list of quote-related documents for the policy.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> QuoteDocuments { get; set; }

        /// <summary>
        /// Gets or sets the list of claim-related documents for the policy.
        /// </summary>
        IEnumerable<ClaimAttachmentReadModel> ClaimDocuments { get; set; }

        /// <summary>
        /// Gets or sets the list of sms associated with the policy.
        /// </summary>
        IEnumerable<ReadWriteModel.Sms> Sms { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the policy.
        /// </summary>
        IEnumerable<Relationship> FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the policy.
        /// </summary>
        IEnumerable<Relationship> ToRelationships { get; set; }
    }
}
