// <copyright file="IPolicyResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Policy
{
    using System;

    /// <summary>
    /// Interface resource model for issuance of Policy.
    /// </summary>
    public interface IPolicyResourceModel
    {
        /// <summary>
        /// Gets the quote ID of the policy the request relates to.
        /// </summary>
        Guid QuoteId { get; }

        /// <summary>
        /// Gets the policy ID of the policy the request relates to.
        /// </summary>
        Guid PolicyId { get; }

        /// <summary>
        /// Gets the policy model.
        /// </summary>
        PolicyModel Policy { get; }
    }
}
