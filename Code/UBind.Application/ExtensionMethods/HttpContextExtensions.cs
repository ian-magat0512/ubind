// <copyright file="HttpContextExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ExtensionMethods
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using UBind.Application.Helpers;

    /// <summary>
    /// Extension methods for <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the client IP of the current Http request.
        /// </summary>
        /// <param name="context">The Http context.</param>
        /// <param name="headerCode">The code to retrieve clientIp address.</param>
        /// <returns>The client IP address.</returns>
        public static IPAddress? GetClientIPAddress(this HttpContext context, string headerCode)
        {
            if (headerCode == null || headerCode.Length != 10)
            {
                throw new InvalidOperationException("header code should be 10 alphanumeric characters long");
            }

            string clientIP = context.Request.Headers["UBind-Client-IP-" + headerCode].ToString();
            return string.IsNullOrEmpty(clientIP)
                ? context.Connection.RemoteIpAddress
                : HeaderParser.GetClientIpAddress(clientIP);
        }
    }
}
