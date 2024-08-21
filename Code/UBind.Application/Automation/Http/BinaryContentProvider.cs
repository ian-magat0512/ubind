// <copyright file="BinaryContentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents a array of byte content defined by a binary provider.
    /// </summary>
    public class BinaryContentProvider : ContentProvider
    {
        private IProvider<Data<byte[]>> content;

        public BinaryContentProvider(IProvider<Data<byte[]>> contentBinary)
        {
            this.content = contentBinary;
        }

        public override string SchemaReferenceKey => "contentBinary";

        public override async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
        {
            var content = (await this.content.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<IData>.Success(new Data<byte[]>(content));
        }
    }
}
