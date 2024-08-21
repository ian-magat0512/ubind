// <copyright file="FileTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System.Text;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// This class is necessary in order to generate text value from a text file referenced by a file provider.
    /// </summary>
    public class FileTextProvider : IProvider<Data<string>>
    {
        private readonly IProvider<Data<FileInfo>> fileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTextProvider"/> class.
        /// </summary>
        /// <param name="fileProvider">The file provider from which the text value is to be obtained from.</param>
        public FileTextProvider(IProvider<Data<FileInfo>> fileProvider)
        {
            this.fileProvider = fileProvider;
        }

        public string SchemaReferenceKey => "fileText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var fileInfo = (await this.fileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var contentArray = fileInfo?.DataValue.Content;
            if (contentArray == null || contentArray.Length == 0)
            {
                return ProviderResult<Data<string>>.Success(null);
            }

            var contentString = Encoding.Default.GetString(contentArray);
            return ProviderResult<Data<string>>.Success(contentString);
        }
    }
}
