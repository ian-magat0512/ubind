// <copyright file="PdfFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.FileHandling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ValueTypes;
    using SystemIo = System.IO;

    /// <summary>
    /// A file provider that replaces merge fields in a doc file with values from the automation data or a specified data object.
    /// </summary>
    public class PdfFileProvider : IProvider<Data<FileInfo>>
    {
        private const string ProviderName = "pdfFile";
        private readonly ILogger<PdfFileProvider> logger;
        private readonly IProvider<Data<string>>? outputFileNameProvider;
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;
        private readonly IPdfEngineService pdfEnginerService;
        private FileInfo sourceFile = null!;
        private string? outputFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFileProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The name to be used for the generated file, if any.</param>
        /// <param name="sourceFile">The source file to be used to merge data on to.</param>
        /// <param name="pdfEngineService">An engine service that handles any pdf related tasks. The reason for having this is to decouple the actual pdf conversion to the provider and also for unit test mocking purposes.</param>
        public PdfFileProvider(
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<FileInfo>> sourceFile,
            IPdfEngineService pdfEngineService,
            ILogger<PdfFileProvider> logger)
        {
            this.logger = logger;
            this.outputFileNameProvider = outputFileName;
            this.sourceFileProvider = sourceFile;
            this.pdfEnginerService = pdfEngineService;
        }

        public string SchemaReferenceKey => "pdfFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            this.logger.LogInformation("Executing \"PdfFileProvider\"");
            this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            this.logger.LogInformation("\"PdfFileProvider\" Source File - " + this.sourceFile.FileName);
            this.outputFileName = (await this.outputFileNameProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.outputFileName = string.IsNullOrEmpty(this.outputFileName?.ToString())
                ? SystemIo.Path.ChangeExtension(this.sourceFile.FileName.ToString(), "pdf")
                : this.outputFileName;
            this.logger.LogInformation("\"PdfFileProvider\" Output File - " + this.outputFileName);

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add("sourceFileName", this.sourceFile.FileName.ToString());
            errorData.Add("outputFileName", this.outputFileName);

            this.logger.LogInformation("\"PdfFileProvider\" calling ValidateingFilesInputs()");
            this.ValidateFilesInputs(errorData);

            this.logger.LogInformation("\"PdfFileProvider\" calling OutputSourceFileBytesToPdfBytes()");
            var outputBytes = this.pdfEnginerService.OutputSourceFileBytesToPdfBytes(
                this.sourceFile, errorData);

            this.logger.LogInformation("\"PdfFileProvider\" calling ThrowIfOutputBytesIsNullOrEmpty()");
            this.ThrowIfOutputBytesIsNullOrEmpty(outputBytes, errorData);

            this.logger.LogInformation("\"PdfFileProvider\" Return File - " + this.outputFileName);
            return ProviderResult<Data<FileInfo>>.Success(new Data<FileInfo>(new FileInfo(this.outputFileName, outputBytes)));
        }

        private void ThrowIfOutputBytesIsNullOrEmpty(byte[] outputBytes, JObject errorData)
        {
            errorData.Add(ErrorDataKey.ErrorMessage, $"Missing output file name: {this.outputFileName}");

            if (outputBytes == null)
            {
                throw new ErrorException(
                    Errors.Automation.PdfFileGenerationFailed(errorData));
            }

            if (outputBytes.Length == 0)
            {
                throw new ErrorException(
                    Errors.Automation.ConvertedFileToPdfIsEmpty(errorData));
            }
        }

        private void ValidateFilesInputs(JObject errorData)
        {
            var additionalDetails = new List<string>();

            var supportedInputFileExtensions = MsWordDocumentHelper.GetSupportedDocumentExtensionByThePlatform();
            var sourceExtension = System.IO.Path.GetExtension(this.sourceFile.FileName.ToString());
            if (!supportedInputFileExtensions.Contains(sourceExtension))
            {
                var extensionDescription = supportedInputFileExtensions
                    .GenerateDescriptionUsingSingleQuoteAndCommaSeparationForEachItem();
                additionalDetails.Add($"Valid input filename extensions: {extensionDescription}");
                throw new ErrorException(
                    Errors.Automation.InputFileHasUnsupportedExtensionForTheCurrentProvider(
                        errorData, additionalDetails, ProviderName));
            }

            var resultOfValidatingInvalidOutputFileType = this.ValidateDefinedOutputFileType();
            if (resultOfValidatingInvalidOutputFileType.IsFailure)
            {
                additionalDetails.Add(resultOfValidatingInvalidOutputFileType.Error);
                throw new ErrorException(Errors.Automation.OutputFileHasUnsupportedExtensionForTheCurrentProvider(
                    errorData, additionalDetails, ProviderName));
            }

            var resultOfInvalidOutputFileDefinition = FileName.ValidateFileName(this.outputFileName!);
            if (resultOfInvalidOutputFileDefinition.IsFailure)
            {
                additionalDetails.Add(resultOfInvalidOutputFileDefinition.Error);
                throw new ErrorException(Errors.File.FileNameInvalid(additionalDetails, errorData));
            }
        }

        private Result<string> ValidateDefinedOutputFileType()
        {
            string? extension = SystemIo.Path.GetExtension(this.outputFileName);
            var expectedExtension = ".pdf";
            if (extension == null || !extension.Equals(expectedExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Failure<string>(
                    $"Valid output filename extensions for {ProviderName} provider: '{expectedExtension}'");
            }

            return Result.Success("succeed");
        }
    }
}
