// <copyright file="FileBinaryProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using System;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="FileBinaryProvider"/> from the JSON configuration.
    /// </summary>
    public class FileBinaryProviderConfigModel : IBuilder<IProvider<Data<byte[]>>>
    {
        public IBuilder<IProvider<Data<FileInfo>>> FileProvider { get; set; }

        public IProvider<Data<byte[]>> Build(IServiceProvider dependencyProvider)
        {
            return new FileBinaryProvider(this.FileProvider.Build(dependencyProvider));
        }
    }
}
