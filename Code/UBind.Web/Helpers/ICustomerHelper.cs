// <copyright file="ICustomerHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System;
    using System.Threading.Tasks;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Helper for resolving customer details in preparation for commands or queries.
    /// </summary>
    public interface ICustomerHelper
    {
        /// <summary>
        /// Resolves missing data for customer details so it's ready for commands and queries.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="customerPersonalDetailsModel">The passed model of customer personal details.</param>
        /// <param name="organisationId">The organisation ID (optinoal).</param>
        /// <param name="portalId">The ID of the portal the customer would login to, if any.</param>
        /// <returns>The resolved customer details.</returns>
        Task<ResolvedCustomerPersonalDetailsModel> CreateResolvedCustomerPersonalDetailsModel(
            Guid tenantId,
            CustomerPersonalDetailsModel customerPersonalDetailsModel,
            Guid? organisationId,
            Guid? portalId = null);
    }
}
