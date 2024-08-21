// <copyright file="ReleaseProductConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;

    /// <summary>
    /// Holds the raw configuration json for a product's release.
    /// </summary>
    public class ReleaseProductConfiguration
    {
        /// <summary>
        /// Gets or sets the raw configuration json for the product at the given release.
        /// </summary>
        public string ConfigurationJson { get; set; }

        /// <summary>
        /// Gets or sets the ID of the release which produced the configuration json.
        /// </summary>
        public Guid ProductReleaseId { get; set; }
    }
}
