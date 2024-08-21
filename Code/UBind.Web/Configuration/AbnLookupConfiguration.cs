// <copyright file="AbnLookupConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Configuration
{
    using UBind.Domain.AbnLookup;

    /// <summary>
    /// This class is needed because we need to access the ABN lookup configuration.
    /// </summary>
    public class AbnLookupConfiguration : IAbnLookupConfiguration
    {
        /// <inheritdoc/>
        public string UBindGuid { get; set; }

        /// <inheritdoc/>
        public string EndpointConfigurationName { get; set; }

        /// <inheritdoc/>
        public string EndpointConfigurationNameRpc { get; set; }
    }
}
