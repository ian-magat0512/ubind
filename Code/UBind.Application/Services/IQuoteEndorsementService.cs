// <copyright file="IQuoteEndorsementService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services;

using System.Threading.Tasks;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Product;

/// <summary>
/// Handles endorsement-related requests.
/// </summary>
public interface IQuoteEndorsementService
{
    /// <summary>
    /// Changes a quote state to approved, used when it doesn't have any referral triggers.
    /// Since only approved quotes can be bound, this needs to be performed before binding.
    /// </summary>
    Task AutoApproveQuote(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles the approval of a quote under review.
    /// </summary>
    Task ApproveReviewedQuote(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles requests for declining a quote that is for approval or completion.
    /// </summary>
    Task DeclineQuote(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles requests for referring a quote for endorsement.
    /// </summary>
    Task ReferQuoteForEndorsement(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles the approval of a quote that is for endorsement.
    /// </summary>
    Task ApproveEndorsedQuote(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles the request for returning a quote to it's previous state.
    /// </summary>
    Task ReturnQuote(ReleaseContext releaseContext, Quote quote, FormData? formData);

    /// <summary>
    /// Handles the request for reviewing a quote under referral.
    /// </summary>
    Task ReferQuoteForReview(ReleaseContext releaseContext, Quote quote, FormData? formData);
}
