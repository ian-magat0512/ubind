// <copyright file="ContentSourceFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;

    public class ContentSourceFileProviderConfigModel : IBuilder<ContentSourceFileProvider>
    {
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        public IBuilder<IProvider<Data<string>>> Alias { get; set; }

        public IBuilder<IProvider<Data<bool>>> IncludeCondition { get; set; }

        public ContentSourceFileProvider Build(IServiceProvider dependencyProvider)
        {
            return new ContentSourceFileProvider(
                this.SourceFile.Build(dependencyProvider),
                this.Alias.Build(dependencyProvider),
                this.IncludeCondition?.Build(dependencyProvider));
        }
    }
}
