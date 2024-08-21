// <copyright file="IHttpContextPropertiesResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Net;
    using System.Security.Claims;

    /// <summary>
    /// This class is needed because we need to know the user who triggered the event.
    /// This is so we can trace up which user performed which action.
    /// </summary>
    public interface IHttpContextPropertiesResolver
    {
        /// <summary>
        /// Gets the performing userId.
        /// </summary>
        Guid? PerformingUserId { get; }

        ClaimsPrincipal PerformingUser { get; }

        IPAddress ClientIpAddress { get; }

        bool IsIpAddressWhitelisted { get; }

        /// <summary>
        /// Check the secret key is passed on header was match on official secret key.
        /// </summary>
        /// <returns>Return true if its valid then false if not.</returns>
        bool IsValidSecretKey();
    }
}
