﻿// <copyright file="ClaimReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is needed because we need a data transfer object for claim and its related entities.
    /// </summary>
    public class ClaimReadModelWithRelatedEntities : IClaimReadModelWithRelatedEntities
    {
        /// <inheritdoc/>
        public ClaimReadModel Claim { get; set; }

        /// <inheritdoc/>
        public UserReadModel? Owner { get; set; }

        /// <inheritdoc/>
        public CustomerReadModel Customer { get; set; }

        /// <inheritdoc/>
        public PolicyReadModel Policy { get; set; }

        /// <inheritdoc/>
        public IEnumerable<PolicyTransaction> PolicyTransactions { get; set; }

        /// <inheritdoc/>
        public Product Product { get; set; }

        /// <inheritdoc/>
        public Tenant Tenant { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ReadWriteModel.Email.Email> Emails { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ClaimVersionReadModel> ClaimVersions { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ProductDetails> ProductDetails { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <inheritdoc/>
        public OrganisationReadModel Organisation { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Sms> Sms { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Relationship> FromRelationships { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Relationship> ToRelationships { get; set; }

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
