// <copyright file="IRedPlanetPremiumFundingApiClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Arteva;

using UBind.Application.Funding.RedPlanetPremiumFunding.Models;

public interface IRedPlanetPremiumFundingApiClient
{
    /// <summary>
    /// The normalized client API url from the configuration
    /// </summary>
    Uri ClientUrl { get; }

    /// <summary>
    /// Sends a POST request to /quotes to create a new quote
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<QuoteDetail> CreateQuote(CreateUpdateQuoteModel model, bool saveQuote, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a POST request to /quotes/:quoteNumber to update quote
    /// </summary>
    /// <param name="model"></param>
    /// <param name="quoteNumber">The returned <see cref="QuoteDetail.QuoteNumber"/> from a previous created quote result.</param>
    /// <returns></returns>
    Task<QuoteDetail> UpdateQuote(CreateUpdateQuoteModel model, string quoteNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a POST request to /quotes/quick-quote to get a quick quote for a single quote
    /// </summary>
    /// <returns></returns>
    Task<QuickQuoteDetail?> QuickQuote(QuickQuoteModel model, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a POST request to /quotes/quick-quote to get a quick quote for multiple quote
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<QuickQuoteDetail>> QuickQuote(IList<QuickQuoteModel> models, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a POST request to /quotes/:quoteNumber/submit to submit quote and get a loan number
    /// </summary>
    /// <param name="model"></param>
    /// <param name="quoteNumber">The returned <see cref="QuoteDetail.QuoteNumber"/> from a previous created quote result.</param>
    /// <returns></returns>
    Task<QuoteDetail> SubmitQuote(QuoteSubmissionModel model, string quoteNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a GET request to /quotes/:quoteNumber/documents to get the documents related to quote number
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<QuoteDocumentModel>> GetQuoteDocuments(string quoteNumber, CancellationToken cancellationToken);
}
