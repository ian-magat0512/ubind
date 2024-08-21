// <copyright file="BinaryFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    public class BinaryFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly IProvider<Data<string>> outputFileName;
        private readonly IProvider<Data<byte[]>> sourceData;

        public BinaryFileProvider(
            IProvider<Data<string>> outputFileNameProvider,
            IProvider<Data<byte[]>> sourceDataProvider)
        {
            this.outputFileName = outputFileNameProvider;
            this.sourceData = sourceDataProvider;
        }

        public string SchemaReferenceKey => "binaryFile";

        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            string fileName = (await this.outputFileName.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            byte[] binaryData = (await this.sourceData.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<FileInfo>>.Success(new FileInfo(fileName, binaryData));
        }
    }
}
