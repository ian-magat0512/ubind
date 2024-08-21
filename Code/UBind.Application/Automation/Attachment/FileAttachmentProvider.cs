// <copyright file="FileAttachmentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Attachment
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// File Attachment Provider.
    /// </summary>
    public class FileAttachmentProvider : IProvider<Data<FileAttachmentInfo>?>
    {
        private ILogger<FileAttachmentProvider> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The filename of attached file.</param>
        /// <param name="sourceFile">The file to be attached.</param>
        /// <param name="includeCondition">The include condition.</param>
        public FileAttachmentProvider(
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<FileInfo>> sourceFile,
            IProvider<Data<bool>>? includeCondition,
            ILogger<FileAttachmentProvider> logger)
        {
            this.logger = logger;
            this.OutputFileName = outputFileName;
            this.SourceFile = sourceFile;
            this.IncludeCondition = includeCondition;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public IProvider<Data<string>>? OutputFileName { get; }

        /// <summary>
        /// Gets the file to be attached.(e.g. TextFile, PdfFile, MsWord, etc).
        /// </summary>
        public IProvider<Data<FileInfo>> SourceFile { get; }

        /// <summary>
        /// Gets  condition if that attachment will be included or not.
        /// </summary>
        public IProvider<Data<bool>>? IncludeCondition { get; }

        public string SchemaReferenceKey => "#fileAttachment";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileAttachmentInfo>?>> Resolve(IProviderContext providerContext)
        {
            var isIncluded = (await this.IncludeCondition.ResolveValueIfNotNull(providerContext))?.DataValue ?? true;
            if (!isIncluded)
            {
                return ProviderResult<Data<FileAttachmentInfo>?>.Success(null);
            }

            var outputFileName = (await this.OutputFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var sourceFile = (await this.SourceFile.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (sourceFile == null)
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "sourceFile",
                    this.SchemaReferenceKey));
            }

            try
            {
                // If OutputFileName is null, use the filename of the text file provider
                var fileName = outputFileName ?? sourceFile.FileName.ToString();
                var mimeType = ContentTypeHelper.GetMimeTypeForFileExtension(fileName);
                return ProviderResult<Data<FileAttachmentInfo>?>
                    .Success(new FileAttachmentInfo(fileName, sourceFile, mimeType, isIncluded));
            }
            catch (ErrorException appException) when (appException.Error.Code.EqualsIgnoreCase("file.name.invalid"))
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add("sourceFilename", sourceFile.FileName.ToString());
                errorData.Add("outputFilename", outputFileName);

                var additionalDetails = appException.Error.AdditionalDetails != null ? new List<string> { appException.Error.AdditionalDetails.Last() } : null;
                throw new ErrorException(
                    Errors.Automation.Provider.FileAttachmentHasInvalidFileName(errorData, additionalDetails));
            }
        }
    }
}
