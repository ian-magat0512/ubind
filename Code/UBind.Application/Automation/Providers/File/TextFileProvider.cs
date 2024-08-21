// <copyright file="TextFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Text File Provider.
    /// </summary>
    public class TextFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly IProvider<Data<string>> outputFileName;
        private readonly IProvider<Data<string>> sourceData;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The file name.</param>
        /// <param name="sourceData">The content of the file.</param>
        public TextFileProvider(IProvider<Data<string>> outputFileName, IProvider<Data<string>> sourceData)
        {
            this.outputFileName = outputFileName;
            this.sourceData = sourceData;
        }

        public string SchemaReferenceKey => "textFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            var fileName = (await this.outputFileName.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var sourceData = (await this.sourceData.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            try
            {
                var content = Encoding.UTF8.GetBytes(sourceData);
                return ProviderResult<Data<FileInfo>>.Success(new FileInfo(fileName, content));
            }
            catch (ErrorException appException) when (appException.Error.Code.EqualsIgnoreCase("file.name.invalid"))
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                var additionalDetails = new List<string>
                {
                    $"Output Filename: {fileName}",
                };

                if (appException.Error.AdditionalDetails?.Count > 1)
                {
                    additionalDetails.Add(appException.Error.AdditionalDetails.Last());
                }
                throw new ErrorException(Errors.Automation.Provider.TextFileHasInvalidFileName(additionalDetails, errorData));
            }
        }
    }
}
