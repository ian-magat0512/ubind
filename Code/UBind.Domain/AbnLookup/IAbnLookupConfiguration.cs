// <copyright file="IAbnLookupConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.AbnLookup
{
    /// <summary>
    /// This interface is needed because we need access to the ABN lookup configuration.
    /// </summary>
    public interface IAbnLookupConfiguration
    {
        /// <summary>
        /// Gets or sets the assigned uBind Guid.
        /// </summary>
        string UBindGuid { get; set; }

        /// <summary>
        /// Gets or sets the endpoint configuration name.
        /// </summary>
        string EndpointConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint configuration name for RPC version.
        /// </summary>
        string EndpointConfigurationNameRpc { get; set; }
    }
}
