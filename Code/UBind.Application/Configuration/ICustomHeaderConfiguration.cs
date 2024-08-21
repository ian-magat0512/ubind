// <copyright file="ICustomHeaderConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    /// <summary>
    /// Configuration for custom headers.
    /// </summary>
    public interface ICustomHeaderConfiguration
    {
        /// <summary>
        /// Gets code to retrieve client ip from the load balancer.
        /// this is appended to UBind-Client-IP-{code} to retrieve ip address from load balancer.
        /// </summary>
        string ClientIpCode { get; }

        /// <summary>
        /// Gets secret key to compare on the custom request header
        /// if it match is authorized else throw not authorized.
        /// </summary>
        string SecretKey { get; }
    }
}
