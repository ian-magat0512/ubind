// <copyright file="IpAddressParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System;
    using System.Net;

    /// <summary>
    /// Parser for extracting IP address from a string containing an IP address with or without a port.
    /// </summary>
    public static class IpAddressParser
    {
        /// <summary>
        /// Try and read the host from an IP address.
        /// </summary>
        /// <param name="ipAddress">A string containing an IP address with or without a port.</param>
        /// <param name="host">When this method succeeds, contains the host.</param>
        /// <returns><c>true</c>, if the host was successfully parsed, otherwise <c>false</c>.</returns>
        public static bool TryGetIPAddress(string ipAddress, out IPAddress host)
        {
            Uri uri;
            if (Uri.TryCreate("tcp://" + ipAddress, UriKind.Absolute, out uri) ||
                Uri.TryCreate(string.Concat("tcp://[" + ipAddress + "]"), UriKind.Absolute, out uri))
            {
                try
                {
                    host = IPAddress.Parse(uri.Host);
                    return true;
                }
                catch (ArgumentNullException)
                {
                }
                catch (FormatException)
                {
                }
            }

            host = null;
            return false;
        }
    }
}
