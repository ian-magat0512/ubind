// <copyright file="FileTextProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This file is for creating an instance of the <see cref="FileTextProvider"/> from the configuration.
    /// </summary>
    public class FileTextProviderConfigModel : IBuilder<IProvider<Data<string>>>
    {
        /// <summary>
        /// Gets or sets the file provider where the text is to be read from.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> File { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<string>> Build(IServiceProvider dependencyProvider)
        {
            return new FileTextProvider(this.File.Build(dependencyProvider));
        }
    }
}
