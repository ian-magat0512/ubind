// <copyright file="UploadedFileProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using UBind.Application.Export;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for uploaded file provider.
    /// </summary>
    public class UploadedFileProviderModel : IExporterModel<IAttachmentProvider>
    {
        /// <summary>
        /// Gets or sets the type of provider the file will be taken from.
        /// </summary>
        /// <remarks>Confirm possible values.</remarks>
        public IExporterModel<ITextProvider> Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the field in the form model the file is uploaded to.
        /// </summary>
        public IExporterModel<ITextProvider> FieldName { get; set; }

        /// <summary>
        /// Gets or sets the optional output file name (with extension) for the attachment.
        /// </summary>
        public IExporterModel<ITextProvider> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the condition that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterCondition> Condition { get; set; }

        /// <summary>
        /// Build a uploaded file provider.
        /// </summary>
        /// <param name="dependencyProvider">
        /// Container for dependencies required when building exporters
        /// .</param>
        /// <param name="productConfiguration">
        /// Contains the per-product configuration
        /// .</param>
        /// <returns>A new uploaded file provider.</returns>
        public IAttachmentProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            var provider = new UploadedFileProvider(
                this.Type?.Build(dependencyProvider, productConfiguration),
                this.FieldName?.Build(dependencyProvider, productConfiguration),
                this.Condition?.Build(dependencyProvider, productConfiguration),
                this.OutputFileName?.Build(dependencyProvider, productConfiguration),
                dependencyProvider.FileAttachmentRepository);

            return provider;
        }
    }
}
