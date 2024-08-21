// <copyright file="FileBinaryProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This class is taking a file reference and returns the binary data from that file.
    /// </summary>
    /// <remarks>Schema key: #fileBinary.</remarks>
    public class FileBinaryProvider : IProvider<Data<byte[]>>
    {
        private readonly IProvider<Data<FileInfo>> fileProvider;

        public FileBinaryProvider(IProvider<Data<FileInfo>> fileProvider)
        {
            this.fileProvider = fileProvider;
        }

        public string SchemaReferenceKey => "fileBinary";

        public async ITask<IProviderResult<Data<byte[]>>> Resolve(IProviderContext providerContext)
        {
            // Note: The checking if file does not exists is already implemented in the file provider.
            var resolveFile = await this.fileProvider.Resolve(providerContext);
            var file = resolveFile.GetValueOrThrowIfFailed().DataValue;
            if (file != null)
            {
                return ProviderResult<Data<byte[]>>.Success(new Data<byte[]>(file.Content));
            }

            return ProviderResult<Data<byte[]>>.Success(null);
        }
    }
}
