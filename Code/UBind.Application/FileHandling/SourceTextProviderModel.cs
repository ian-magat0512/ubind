// <copyright file="SourceTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using UBind.Application.Export;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for source text file provider.
    /// </summary>
    public class SourceTextProviderModel : IExporterModel<IAttachmentProvider>
    {
        /// <summary>
        /// Gets or sets the name of the output file (with extension).
        /// </summary>
        public IExporterModel<ITextProvider> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the mime type.
        /// </summary>
        public IExporterModel<ITextProvider> MimeType { get; set; }

        /// <summary>
        /// Gets or sets the source text provider.
        /// </summary>
        public IExporterModel<ITextProvider> SourceText { get; set; }

        /// <summary>
        /// Gets or sets the condition that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterCondition> Condition { get; set; }

        /// <summary>
        /// Build a source text file provider.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required when building exporters.</param>
        /// <param name="productConfiguration">
        /// Contains the per-product configuration
        /// .</param>
        /// <returns>The new source text file provider.</returns>
        public IAttachmentProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new SourceTextFileProvider(
                this.SourceText.Build(dependencyProvider, productConfiguration),
                this.OutputFileName?.Build(dependencyProvider, productConfiguration),
                this.MimeType?.Build(dependencyProvider, productConfiguration),
                this.Condition?.Build(dependencyProvider, productConfiguration),
                dependencyProvider.FileContentsLoader);
        }
    }
}
