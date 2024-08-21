// <copyright file="QuoteDocumentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

/// <summary>
/// Quote document model received from Red Planet Odyssey API
/// Endpoint: /quotes/:quoteNumber/documents
/// </summary>
public class QuoteDocumentModel
{
    public QuoteDocumentModel(string name, string accessCode)
    {
        this.Name = name;
        this.AccessCode = accessCode;
    }

    public string Name { get; set; }

    public string AccessCode { get; set; }
}
