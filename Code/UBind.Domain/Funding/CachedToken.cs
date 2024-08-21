// <copyright file="CachedToken.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Funding;

public class CachedToken
{
    public CachedToken(string token, DateTimeOffset expiryTime)
    {
        this.Token = token;
        this.ExpiryTime = expiryTime;
    }

    public string Token { get; }

    public DateTimeOffset ExpiryTime { get; }
}
