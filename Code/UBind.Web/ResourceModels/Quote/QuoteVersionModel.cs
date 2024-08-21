// <copyright file="QuoteVersionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Domain.Extensions;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Web.ResourceModels;

/// <summary>
/// Resource model for serving the details of the quote version.
/// </summary>
public class QuoteVersionModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteVersionModel"/> class.
    /// </summary>
    /// <param name="quoteVersionDetails">The quote version.</param>
    /// <param name="product">The product.</param>
    public QuoteVersionModel(IQuoteVersionReadModelDetails quoteVersionDetails, Product product)
    {
        this.Id = quoteVersionDetails.QuoteVersionId;
        this.OrganisationId = quoteVersionDetails.OrganisationId;
        this.QuoteId = quoteVersionDetails.QuoteId;
        this.ProductId = product.Id;
        this.ProductAlias = product.Details.Alias;
        this.QuoteNumber = quoteVersionDetails.QuoteNumber;
        this.QuoteVersionNumber = quoteVersionDetails.QuoteVersionNumber;
        this.LatestFormData = quoteVersionDetails.LatestFormData;
        this.CustomerId = quoteVersionDetails.CustomerId;
        this.CustomerPersonId = quoteVersionDetails.CustomerPersonId;
        this.CustomerFullName = quoteVersionDetails.CustomerFullName;
        this.CreatedDateTime = quoteVersionDetails.CreatedTimestamp.ToString();
        this.LastModifiedDateTime = quoteVersionDetails.LastModifiedTimestamp.ToString();
        this.CalculationResultJson = quoteVersionDetails.CalculationResultJson;
        this.CalculationResultJson = quoteVersionDetails.CalculationResultJson?.FixMalformedJsonString();
        var calculationResult = quoteVersionDetails.CalculationResult;
        if (calculationResult != null)
        {
            this.PremiumData = new PremiumResult(calculationResult.PayablePrice);
        }
        else if (quoteVersionDetails.CalculationResultJson != null)
        {
            var jsonObject = JObject.Parse(this.CalculationResultJson);
            this.CalculationResultJson = jsonObject.ToString();
            var premiumString = JsonConvert.SerializeObject(jsonObject?["payment"]?["total"]) ?? string.Empty;
            this.PremiumData = JsonConvert.DeserializeObject<PremiumResult>(premiumString) ?? null;
        }

        this.State = quoteVersionDetails.State;
        this.WorkflowStep = quoteVersionDetails.WorkflowStep;
        this.Documents = quoteVersionDetails.Documents.Select(d => new DocumentSetModel(d)).ToList();
    }

    /// <summary>
    /// Gets the id of the quote version.
    /// </summary>
    [JsonProperty]
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the ID of the organisation this quote was created under.
    /// </summary>
    public Guid OrganisationId { get; private set; }

    /// <summary>
    /// Gets the ID of the quote this version was created from.
    /// </summary>
    [JsonProperty]
    public Guid QuoteId { get; private set; }

    /// <summary>
    /// Gets the Product Id the quote is created from.
    /// </summary>
    [JsonProperty]
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Gets the Product Alias the quote is created from.
    /// </summary>
    [JsonProperty]
    public string ProductAlias { get; private set; }

    /// <summary>
    /// Gets the deployment environment of the quote.
    /// </summary>
    [JsonProperty]
    public string Environment { get; private set; }

    /// <summary>
    /// Gets the version number of the quote version.
    /// </summary>
    [JsonProperty]
    public int QuoteVersionNumber { get; private set; }

    /// <summary>
    /// Gets the quote number of the quote.
    /// </summary>
    [JsonProperty]
    public string QuoteNumber { get; private set; }

    /// <summary>
    /// Gets the latest form data when the quote version was saved.
    /// </summary>
    [JsonProperty]
    public string LatestFormData { get; private set; }

    /// <summary>
    /// Gets the Id of the customer the quote is assigned to.
    /// </summary>
    [JsonProperty]
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// Gets the ID of the person who is the customer for the quote.
    /// </summary>
    [JsonProperty]
    public Guid? CustomerPersonId { get; private set; }

    /// <summary>
    /// Gets the full name of the customer for the quote.
    /// </summary>
    [JsonProperty]
    public string CustomerFullName { get; private set; }

    /// <summary>
    /// Gets the latest payment calculation data for the quote.
    /// </summary>
    [JsonProperty]
    public PremiumResult PremiumData { get; private set; }

    /// <summary>
    /// Gets the time that this quote version was created.
    /// </summary>
    [JsonProperty]
    public string CreatedDateTime { get; private set; }

    /// <summary>
    /// Gets the time that this quote version was last modified.
    /// </summary>
    [JsonProperty]
    public string LastModifiedDateTime { get; private set; }

    /// <summary>
    /// Gets the latest calculation result related to this quote version.
    /// </summary>
    [JsonProperty]
    public string CalculationResultJson { get; private set; }

    /// <summary>
    /// Gets the documents associated with this quote version.
    /// </summary>
    public List<DocumentSetModel> Documents { get; private set; }

    /// <summary>
    /// Gets the quote state at the time this quote version was created.
    /// </summary>
    public string State { get; }

    /// <summary>
    /// Gets the quote workflow step at the time this quote version was created.
    /// </summary>
    public string WorkflowStep { get; }
}
