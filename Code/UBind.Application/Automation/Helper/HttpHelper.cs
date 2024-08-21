// <copyright file="HttpHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public static class HttpHelper
    {
        public static void ThrowIfHttpVerbInvalid(string httpVerb)
        {
            Regex regex = new Regex("^[A-Z][A-Z-]*[A-Z]$");
            if (!regex.IsMatch(httpVerb.ToUpper()))
            {
                throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(
                    new JObject()
                    {
                        { ErrorDataKey.ErrorMessage, $"The value \"{httpVerb}\" is not a valid HTTP Verb" },
                    }));
            }
        }

        public static bool IsTextMediaType(MediaTypeHeaderValue header)
        {
            string mediaType = header?.MediaType?.ToString();
            return mediaType != null
                && (mediaType.StartsWith("text") || mediaType.StartsWith("application/json")
                    || mediaType.StartsWith("application/javascript") || mediaType.Contains("charset="));
        }

        public static bool IsJsonMediaType(MediaTypeHeaderValue header)
        {
            string mediaType = header?.MediaType?.ToString();
            return mediaType != null && (mediaType.Contains("/json") || mediaType.Contains("+json"));
        }

        public static bool IsMultipartMediaType(MediaTypeHeaderValue header)
        {
            string mediaType = header?.MediaType?.ToString();
            return mediaType != null && mediaType.StartsWith("multipart");
        }
    }
}
