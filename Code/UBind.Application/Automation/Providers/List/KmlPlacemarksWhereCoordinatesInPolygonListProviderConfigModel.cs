// <copyright file="KmlPlacemarksWhereCoordinatesInPolygonListProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;

    /// <summary>
    /// Model for building an instance of <see cref="KmlPlacemarksWhereCoordinatesInPolygonListProvider"/>.
    /// </summary>
    public class KmlPlacemarksWhereCoordinatesInPolygonListProviderConfigModel : IBuilder<IDataListProvider<object>>
    {
        /// <summary>
        /// Gets or sets the builder for getting the longitude.
        /// </summary>
        public IBuilder<IProvider<Data<decimal>>> Longitude { get; set; }

        /// <summary>
        /// Gets or sets the builder for getting the latitude.
        /// </summary>
        public IBuilder<IProvider<Data<decimal>>> Latitude { get; set; }

        /// <summary>
        /// Gets or sets the builder for getting the Kml Data.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> KmlData { get; set; }

        /// <inheritdoc/>
        public IDataListProvider<object> Build(IServiceProvider dependencyProvider)
        {
            return new KmlPlacemarksWhereCoordinatesInPolygonListProvider(
                this.Latitude.Build(dependencyProvider),
                this.Longitude.Build(dependencyProvider),
                this.KmlData.Build(dependencyProvider));
        }
    }
}
