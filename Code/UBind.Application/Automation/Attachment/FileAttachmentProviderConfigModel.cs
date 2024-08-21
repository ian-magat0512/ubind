// <copyright file="FileAttachmentProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Attachment
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// Defines an email attachment provider.
    /// </summary>
    public class FileAttachmentProviderConfigModel : IBuilder<IProvider<Data<FileAttachmentInfo>?>>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the file that will be attached to the email, as specified by a file provider.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the file the new file name for the attachment.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the condition, if available, used to determine if this attachment can be included.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>>? IncludeCondition { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileAttachmentInfo>?> Build(IServiceProvider dependencyProvider)
        {
            var logging = dependencyProvider.GetRequiredService<ILogger<FileAttachmentProvider>>();
            return new FileAttachmentProvider(
                this.OutputFileName?.Build(dependencyProvider),
                this.SourceFile.Build(dependencyProvider),
                this.IncludeCondition?.Build(dependencyProvider), logging);
        }
    }
}
