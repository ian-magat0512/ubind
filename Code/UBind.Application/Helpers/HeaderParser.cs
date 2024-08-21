// <copyright file="HeaderParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Utility for extracting client IP address from an Http requests's header.
    /// </summary>
    public static class HeaderParser
    {
        /// <summary>
        /// Gets the client IP address from an Http requests's header.
        /// </summary>
        /// <param name="headerValue">The value of a request's header.</param>
        /// <returns>The IP address of the client.</returns>
        /// <exception cref="FormatException">Thrown if the header is not in an expected format.</exception>
        public static IPAddress GetClientIpAddress(string headerValue)
        {
            // header is expected to be comma-separated list of IP addresses with or without ports.
            headerValue.ThrowIfArgumentNullOrWhitespace(nameof(headerValue));

            // First value in comma-separated list will be original client.
            var clientAddress = headerValue
                .Split(',')
                .Select(address => address.Trim())
                .First();

            // Trim port, if present.
            if (IpAddressParser.TryGetIPAddress(clientAddress, out IPAddress host))
            {
                return host;
            }

            throw new ErrorException(Errors.General.BadRequest($"Client IP header is in unexpected format: \"{headerValue}\""));
        }
    }
}
