// <copyright file="TextFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;

    /// <summary>
    /// Model for creating an instance of <see cref="TextFileProvider"/>.
    /// </summary>
    public class TextFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the file content.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> SourceData { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            return new TextFileProvider(this.OutputFileName.Build(dependencyProvider), this.SourceData.Build(dependencyProvider));
        }
    }
}
