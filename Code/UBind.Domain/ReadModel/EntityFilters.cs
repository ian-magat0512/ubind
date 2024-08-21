// <copyright file="EntityFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using UBind.Domain.Enums;

    /// <summary>
    /// Filter models in getting entities such as Quote, Quote Versions, Claim, Claim Version, etc
    /// (please refer <see cref="AdditionalPropertyDefinitionType"/> for the complete list) for additional property's
    /// related transaction. This is normally being used to get the collection of entity ids but can also be used for
    /// something else in the future.
    /// </summary>
    public class EntityFilters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFilters"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID (optional).</param>
        /// <param name="organisationId">The organisation ID (optional).</param>
        /// <param name="skip">The current page index to search for (optional).</param>
        /// <param name="pageSize">The max count per page (optional).</param>
        public EntityFilters(
            Guid tenantId,
            Guid? productId = null,
            Guid? organisationId = null,
            int? skip = null,
            int? pageSize = null)
        {
            this.TenantId = tenantId;
            this.Skip = skip;
            this.PageSize = pageSize;
            this.ProductId = productId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the value of the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the value of the current page index.
        /// </summary>
        public int? Skip { get; }

        /// <summary>
        /// Gets the value of the max cound per page.
        /// </summary>
        public int? PageSize { get; }

        /// <summary>
        /// Gets the value of the organisation ID.
        /// </summary>
        public Guid? OrganisationId { get; }

        /// <summary>
        /// Gets the value of the product ID.
        /// </summary>
        public Guid? ProductId { get; }
    }
}
