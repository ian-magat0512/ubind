// <copyright file="AccessTokenResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Models;

using System.Text.Json.Serialization;

public class AccessTokenResponse
{
    /// <summary>
    /// The access token value
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets the token type.
    /// </summary>
    /// <remarks>Not used. Expected to always be "Bearer".</remarks>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
