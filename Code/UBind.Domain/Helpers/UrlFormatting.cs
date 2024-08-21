// <copyright file="UrlFormatting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Web;
    using Humanizer;

    /// <summary>
    /// Parser for Url formatting.
    /// </summary>
    public class UrlFormatting
    {
        /// <summary>
        /// Gets the Assets url.
        /// </summary>
        /// <param name="baseUrl">The base url.</param>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <param name="environment">The Environment.</param>
        /// <returns>converted string.</returns>
        public static string GetAssetsUrl(
            string baseUrl,
            string tenantAlias,
            string productAlias,
            DeploymentEnvironment environment,
            WebFormAppType formType,
            Guid? releaseId)
        {
            string path = $"api/v1"
                + $"/tenant{tenantAlias}"
                + $"/product/{productAlias}"
                + $"/environment/{environment.ToString().ToLower()}"
                + $"/form-type/{formType.Humanize().ToLowerInvariant()}"
                + (releaseId != null ? $"/release/{releaseId}" : string.Empty)
                + $"/asset";
            if (baseUrl == null)
            {
                return string.Empty;
            }

            return $"{baseUrl}".TrimEnd('/') + $"/{path}";
        }

        /// <summary>
        /// TODO: Remove and all its implementations once product templates has been fixed.
        /// </summary>
        /// <param name="baseUrl">the base url.</param>
        /// <returns>Portal Url string.</returns>
        public static string GetApplicationUrl(string baseUrl)
        {
            if (baseUrl == null)
            {
                return string.Empty;
            }

            return $"{baseUrl}".TrimEnd('/');
        }

        /// <summary>
        /// Appends a query parameter to a url.
        /// Note, it UrlEncodes any special characters in the value.
        /// </summary>
        /// <param name="url">The url to append the query parameter to.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The url with the query parameter appended to it.</returns>
        public static string AppendQueryParameter(string url, string name, string value)
        {
            string separator = url.Contains('?') ? "&" : "?";
            return url + separator + $"{name}={HttpUtility.UrlEncode(value)}";
        }
    }
}
