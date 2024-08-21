// <copyright file="ExtractFromArchiveFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;

    public class ExtractFromArchiveFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        /// <summary>
        /// Gets or sets the builder of the provider of the source file to read from.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the password of the archive that is being read from.
        /// If the archive file is protected by a password, that password can be specified here using this optional
        /// parameter.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Password { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the password of the archive that is being read from.
        /// The file format of the source file. If omitted the format will be detected based on the filename
        /// extension. Initially only 'zip' is supported.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Format { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the path of the file in the archive.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> FilePath { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the output filename for the file.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> OutputFilename { get; set; }

        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            return new ExtractFromArchiveFileProvider(
                this.SourceFile.Build(dependencyProvider),
                this.Password?.Build(dependencyProvider),
                this.Format?.Build(dependencyProvider),
                this.FilePath.Build(dependencyProvider),
                this.OutputFilename?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
