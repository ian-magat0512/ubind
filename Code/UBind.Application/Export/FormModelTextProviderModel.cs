// <copyright file="FormModelTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for a form field text provider.
    /// </summary>
    public class FormModelTextProviderModel : IExporterModel<ITextProvider>
    {
        /// <summary>
        /// Gets or sets the name of the form field to get text from.
        /// </summary>
        public IExporterModel<ITextProvider> FieldName { get; set; }

        /// <summary>
        /// Build a form field text provider.
        /// </summary>
        /// <param name="dependencyProvider">Container providing dependencies required in exporter building.</param>
        /// <param name="productConfiguration">Contains the per-product configuration.</param>
        /// <returns>A new form field text provider.</returns>
        public ITextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new FormModelTextProvider();
        }
    }
}
