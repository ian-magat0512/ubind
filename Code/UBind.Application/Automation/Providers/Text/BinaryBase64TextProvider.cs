// <copyright file="BinaryBase64TextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    public class BinaryBase64TextProvider : IProvider<Data<string>>
    {
        private readonly IProvider<Data<byte[]>> binaryProvider;

        public BinaryBase64TextProvider(IProvider<Data<byte[]>> binaryProvider)
        {
            this.binaryProvider = binaryProvider;
        }

        public string SchemaReferenceKey => "binaryBase64Text";

        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var data = (await this.binaryProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            return ProviderResult<Data<string>>.Success(new Data<string>(Convert.ToBase64String(data.DataValue)));
        }
    }
}
