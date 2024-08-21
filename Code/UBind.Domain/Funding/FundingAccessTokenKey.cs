// <copyright file="FundingAccessTokenKey.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Funding;

/// <summary>
/// The object used to create the identifier key for persisting and retrieving third-party access token used by a funding provider
/// </summary>
public class FundingAccessTokenKey
{
    public FundingAccessTokenKey(string username, FundingServiceName serviceName, string? fundingKey = null)
    {
        this.Username = username;
        this.FundingKey = fundingKey;
        this.ServiceName = serviceName;
    }

    public string Username { get; set; }

    /// <summary>
    /// An optional additional key for purposes where a sub-implementation of a Funding Provider exist
    /// Eg. Red Planet - Arteva
    /// </summary>
    public string? FundingKey { get; private set; }

    public FundingServiceName ServiceName { get; private set; }

    public override string ToString()
    {
        var fundingKey = this.FundingKey != null ? this.FundingKey + ":" : string.Empty;
        return $"{this.ServiceName}:{fundingKey}{this.Username}";
    }
}
