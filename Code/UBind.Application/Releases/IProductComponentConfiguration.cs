// <copyright file="IProductComponentConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// For holding configuration specific to product components.
    /// </summary>
    public interface IProductComponentConfiguration
    {
        /// <summary>
        /// Gets the assets associated with the release.
        /// </summary>
        IReadOnlyList<Asset> Assets { get; }

        /// <summary>
        /// Gets the component of the product, if available, otherwise null.
        /// The product component will only be available fo products with configurations
        /// of version 2 or later.
        /// </summary>
        Component Component { get; }

        /// <summary>
        /// Gets the files associated with the release.
        /// </summary>
        IReadOnlyList<Asset> Files { get; }

        /// <summary>
        /// Gets the form data schema.
        /// </summary>
        Maybe<FormDataSchema> FormDataSchema { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration version is 1.
        /// </summary>
        bool IsVersion1 { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration version is 2 or greater.
        /// </summary>
        bool IsVersion2OrGreater { get; }

        /// <summary>
        /// Gets the product configuration.
        /// </summary>
        IProductConfiguration ProductConfiguration { get; }

        /// <summary>
        /// Gets the configuration version, e.g. "2.0.0".
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets the configuration json for the web form app.
        /// </summary>
        string WebFormAppConfigurationJson { get; }

        /// <summary>
        /// Gets the contents of the workbook for the web form app.
        /// </summary>
        byte[] WorkbookData { get; }
    }
}
