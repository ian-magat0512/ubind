// <copyright file="IPolicyOwnershipDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// Data required to determine policy access.
    /// </summary>
    public interface IPolicyOwnershipDetails
    {
        /// <summary>
        /// Gets the ID of the policy.
        /// </summary>
        Guid PolicyId { get; }

        /// <summary>
        /// Gets the ID of the organisation the policy is in.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the ID of the product the policy is for.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the ID of the user who owns the policy.
        /// </summary>
        Guid? OwnerUserId { get; }

        /// <summary>
        /// Gets the ID of the customer for the policy.
        /// </summary>
        Guid? CustomerId { get; }
    }
}
