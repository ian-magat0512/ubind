// <copyright file="FixedTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for fixed text provider.
    /// </summary>
    public class FixedTextProviderModel : IExporterModel<ITextProvider>
    {
        /// <summary>
        /// Gets or sets the text to provide.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Build a fixed text provider.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required for exporter building.</param>
        /// <param name="productConfiguration">Contains per-product configuration.</param>
        /// <returns>The new fixed text provider.</returns>
        public ITextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new FixedTextProvider(this.Text);
        }
    }
}
