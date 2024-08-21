// <copyright file="HttpRequestExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ExtensionMethods
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;

    public static class HttpRequestExtensions
    {
        public static string GetDomain(this IHttpContextAccessor contextAccessor)
        {
            var uri = GetURI(contextAccessor);
            return uri?.Host;
        }

        public static string GetBaseUrl(this IHttpContextAccessor contextAccessor)
        {
            var uri = GetURI(contextAccessor);
            return uri != null ? $"{uri.Scheme}://{uri.Host}/" : null;
        }

        private static Uri GetURI(IHttpContextAccessor contextAccessor)
        {
            return contextAccessor.HttpContext != null ? new Uri(contextAccessor.HttpContext?.Request.GetEncodedUrl()) : null;
        }
    }
}
