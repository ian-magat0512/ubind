// <copyright file="ArchiveFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;

    public class ArchiveFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        /// <summary>
        /// Gets or sets the provider of the source archive file.
        /// An optional archive file that the output file should be based on.
        /// If omitted, a new archive file will be created.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the password of the archive that is being read from.
        /// If the archive file is protected by a password, that password can be specified here using this optional
        /// parameter.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> SourceFilePassword { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the format of the archive file that is being created or
        /// opened. When using a sourceFile it is optional to specify the fileFormat. In that case, if omitted
        /// the format will be detected based on the filename extension.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Format { get; set; }

        /// <summary>
        /// Gets or sets the filename of the new archive file to be created, if a new one is being created.
        /// Otherwise, the filename of the source file is used.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the builder of the provider for the password of the output archive.
        /// If used, the provided text value will be set as a password on the output archive file. If a source archive
        /// file with a password is used, this will replace that password. If a null value is provided, an existing
        /// password will be removed.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Password { get; set; }

        /// <summary>
        /// Gets or sets a list of builders for operations to apply to perform on archive file.
        /// </summary>
        public IEnumerable<IBuilder<ArchiveFile.Operation>> Operations { get; set; }
            = Enumerable.Empty<IBuilder<ArchiveFile.Operation>>();

        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            return new ArchiveFileProvider(
                this.SourceFile?.Build(dependencyProvider),
                this.SourceFilePassword?.Build(dependencyProvider),
                this.Format?.Build(dependencyProvider),
                this.OutputFileName?.Build(dependencyProvider),
                this.Password?.Build(dependencyProvider),
                this.Operations?.Select(o => o.Build(dependencyProvider)),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
