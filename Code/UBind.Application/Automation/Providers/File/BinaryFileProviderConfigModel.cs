// <copyright file="BinaryFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;

    public class BinaryFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        public IBuilder<IProvider<Data<string>>> OutputFileName { get; set; }

        public IBuilder<IProvider<Data<byte[]>>> SourceData { get; set; }

        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            return new BinaryFileProvider(this.OutputFileName.Build(dependencyProvider), this.SourceData.Build(dependencyProvider));
        }
    }
}
