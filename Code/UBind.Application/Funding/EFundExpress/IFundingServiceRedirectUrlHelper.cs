// <copyright file="IFundingServiceRedirectUrlHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress
{
    using System;
    using UBind.Domain.Product;

    /// <summary>
    /// Helper for generating URLs to provide to funding site for redirecting back to UBind.
    /// </summary>
    public interface IFundingServiceRedirectUrlHelper
    {
        /// <summary>
        /// The URL the funding provider will redirect to after funding has been successfully accepted.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="proposalId">The ID of the relevant proposal.</param>
        /// <returns>The URL to pass to the funding service.</returns>
        string GetSuccessRedirectUrl(IProductContext productContext, Guid quoteId, string proposalId);

        /// <summary>
        /// The URL the funding provider will redirect to after funding attempt has been cancelled.
        /// </summary>
        /// <param name="quoteId">The ID of the quote the funding is for.</param>
        /// <param name="proposalId">The ID of the relevant proposal.</param>
        /// <returns>The URL to pass to the funding service.</returns>
        string GetCancellationRedirectUrl(IProductContext productContext, Guid quoteId, string proposalId);
    }
}
