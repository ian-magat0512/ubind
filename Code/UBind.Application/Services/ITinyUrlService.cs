// <copyright file="ITinyUrlService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services;

using UBind.Domain;

public interface ITinyUrlService
{
    /// <summary>
    /// Generates a tiny URL token and persists it with a redirect URL and other information in the database.
    /// This token then can be used to retrieve the redirect URL.
    /// </summary>
    Task<string> GenerateAndPersistUrl(Guid tenantId, DeploymentEnvironment environment, string redirectUrl);

    /// <summary>
    /// Gets the redirect url for the given token.
    /// </summary>
    Task<string?> GetRedirectUrl(string token);
}
