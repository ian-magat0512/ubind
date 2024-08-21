// <copyright file="IExporterModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for a text provider.
    /// </summary>
    /// <typeparam name="T">Type object this model builds.</typeparam>
    public interface IExporterModel<T>
    {
        /// <summary>
        /// Build a text provider based on the model.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependcies used for building exporters.</param>
        /// <param name="productConfiguration">The product config.</param>
        /// <returns>A new text provider.</returns>
        T Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration);
    }
}
