// <copyright file="SavedFileProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using UBind.Application.Export;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for saved file provider.
    /// </summary>
    public class SavedFileProviderModel : IExporterModel<IAttachmentProvider>
    {
        /// <summary>
        /// Gets or sets the source object for the saved document.
        /// </summary>
        public string SourceObject { get; set; }

        /// <summary>
        /// Gets or sets the file name of the document as saved in the sytem.
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the output file name to be used.
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the condition that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterCondition> Condition { get; set; }

        /// <summary>
        /// Build a saved file provider.
        /// </summary>
        /// <param name="dependencyProvider">
        /// Container for dependencies required when building exporters
        /// .</param>
        /// <param name="productConfiguration">
        /// Contains the per-product configuration
        /// .</param>
        /// <returns>A new saved file provider.</returns>
        public IAttachmentProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new SavedFileProvider(
                this.SourceObject,
                this.SourceFileName,
                this.OutputFileName,
                this.Condition?.Build(dependencyProvider, productConfiguration),
                dependencyProvider.DocumentService);
        }
    }
}
