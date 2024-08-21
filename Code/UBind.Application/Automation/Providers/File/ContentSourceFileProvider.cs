// <copyright file="ContentSourceFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    public class ContentSourceFileProvider : IProvider<Data<ContentSourceFile>>
    {
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;

        private readonly IProvider<Data<string>> aliasProvider;

        private readonly IProvider<Data<bool>> includeConditionProvider;

        public ContentSourceFileProvider(
            IProvider<Data<FileInfo>> sourceFileProvider,
            IProvider<Data<string>> aliasProvider,
            IProvider<Data<bool>> includeConditionProvider)
        {
            this.sourceFileProvider = sourceFileProvider;
            this.aliasProvider = aliasProvider;
            this.includeConditionProvider = includeConditionProvider;
        }

        public string SchemaReferenceKey => "contentSourceFile";

        public async ITask<IProviderResult<Data<ContentSourceFile>>> Resolve(IProviderContext providerContext)
        {
            var file = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var alias = (await this.aliasProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            bool includeCondition = true;
            if (this.includeConditionProvider != null)
            {
                includeCondition = (await this.includeConditionProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            }

            return ProviderResult<Data<ContentSourceFile>>.Success(new ContentSourceFile(file, alias, includeCondition));
        }
    }
}
