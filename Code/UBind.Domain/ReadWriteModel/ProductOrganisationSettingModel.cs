// <copyright file="ProductOrganisationSettingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;

    /// <summary>
    /// Model to hold the product setting within the organisation.
    /// </summary>
    public class ProductOrganisationSettingModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductOrganisationSettingModel"/> class.
        /// </summary>
        /// <param name="name">The product name.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="isNewQuotesAllowed">The flag to determine whether new quotes are allowed.</param>
        /// <param name="createdTicksSinceEpoch">The created time in ticks since epoch.</param>
        public ProductOrganisationSettingModel(
            string name,
            Guid organisationId,
            Guid productId,
            bool isNewQuotesAllowed,
            long createdTicksSinceEpoch)
        {
            this.Name = name;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.IsNewQuotesAllowed = isNewQuotesAllowed;
            this.CreatedTicksSinceEpoch = createdTicksSinceEpoch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductOrganisationSettingModel"/> class.
        /// </summary>
        public ProductOrganisationSettingModel()
        {
        }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; }

        /// <summary>
        /// Gets a value indicating whether creation of new quotes for a Product is allowed.
        /// </summary>
        public bool IsNewQuotesAllowed { get; }

        /// <summary>
        /// Gets the created time in ticks since epoch.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; }
    }
}
