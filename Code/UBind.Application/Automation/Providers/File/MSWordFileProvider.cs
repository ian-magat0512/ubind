// <copyright file="MSWordFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling;
    using UBind.Application.FileHandling.GemBoxServices;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// A file provider that replaces merge fields in a word file with values from the automation data or a specified data object.
    /// </summary>
    public class MSWordFileProvider : IProvider<Data<FileInfo>>
    {
        private const string ProviderName = "msWordFile";
        private readonly ILogger<MSWordFileProvider> logger;
        private readonly IProvider<Data<string>>? outputFileNameProvider;
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;
        private readonly IObjectProvider dataObjectProvider;
        private readonly IProvider<Data<bool>> flattenDataObjectProvider;
        private readonly IProvider<Data<long>> repeatingRangeStartIndexProvider;
        private readonly IEnumerable<ContentSourceFileProvider> contentProviders;
        private readonly IProvider<Data<bool>> removeUnusedMergeFieldsProvider;
        private readonly IProvider<Data<bool>> removeRangesWhereAllMergeFieldsAreUnusedProvider;
        private readonly IProvider<Data<bool>> removeTablesWhereAllMergeFieldsAreUnusedProvider;
        private readonly IProvider<Data<bool>> removeTableRowsWhereAllMergeFieldsAreUnusedProvider;
        private readonly IProvider<Data<bool>> removeParagraphsWhereAllMergeFieldsAreUnusedProvider;

        private IProviderContext providerContext;
        private FileInfo sourceFile;
        private string outputFileName;
        private JObject dataObject;
        private bool flattenDataObject;
        private long repeatingRangeStartIndex;
        private List<ContentSourceFile> content;
        private bool removeUnusedMergeFields;
        private bool removeRangesWhereAllMergeFieldsAreUnused;
        private bool removeTablesWhereAllMergeFieldsAreUnused;
        private bool removeTableRowsWhereAllMergeFieldsAreUnused;
        private bool removeParagraphsWhereAllMergeFieldsAreUnused;
        private IGemBoxMsWordEngineService gemBoxMsWordEngineService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSWordFileProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The name to be used for the generated file, if any.</param>
        /// <param name="sourceFile">The source file to be used to merge data on to.</param>
        /// <param name="dataObject">The data object to be used for generating the new file, if any.</param>
        /// <param name="gemBoxDocumentEngineService">The MS Word engine service.</param>
        public MSWordFileProvider(
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<FileInfo>> sourceFile,
            IObjectProvider dataObject,
            IProvider<Data<bool>> flattenDataObjectProvider,
            IProvider<Data<long>> repeatingRangeStartIndexProvider,
            IEnumerable<ContentSourceFileProvider> contentProviders,
            IProvider<Data<bool>> removeUnusedMergeFieldsProvider,
            IProvider<Data<bool>> removeRangesWhereAllMergeFieldsAreUnusedProvider,
            IProvider<Data<bool>> removeTablesWhereAllMergeFieldsAreUnusedProvider,
            IProvider<Data<bool>> removeTableRowsWhereAllMergeFieldsAreUnusedProvider,
            IProvider<Data<bool>> removeParagraphsWhereAllMergeFieldsAreUnusedProvider,
            IGemBoxMsWordEngineService gemBoxMsWordEngineService,
            ILogger<MSWordFileProvider> logger)
        {
            Contract.Assert(sourceFile != null);

            this.logger = logger;
            this.outputFileNameProvider = outputFileName;
            this.sourceFileProvider = sourceFile;
            this.dataObjectProvider = dataObject;
            this.flattenDataObjectProvider = flattenDataObjectProvider;
            this.repeatingRangeStartIndexProvider = repeatingRangeStartIndexProvider;
            this.contentProviders = contentProviders;
            this.removeUnusedMergeFieldsProvider = removeUnusedMergeFieldsProvider;
            this.removeRangesWhereAllMergeFieldsAreUnusedProvider
                = removeRangesWhereAllMergeFieldsAreUnusedProvider;
            this.removeTablesWhereAllMergeFieldsAreUnusedProvider
                = removeTablesWhereAllMergeFieldsAreUnusedProvider;
            this.removeTableRowsWhereAllMergeFieldsAreUnusedProvider
                = removeTableRowsWhereAllMergeFieldsAreUnusedProvider;
            this.removeParagraphsWhereAllMergeFieldsAreUnusedProvider
                = removeParagraphsWhereAllMergeFieldsAreUnusedProvider;
            this.gemBoxMsWordEngineService = gemBoxMsWordEngineService;
            this.content = new List<ContentSourceFile>();
        }

        public string SchemaReferenceKey => "msWordFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            this.logger.LogInformation("Executing \"MSWordFileProvider\"");
            this.providerContext = providerContext;
            this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            this.logger.LogInformation($"\"MSWordFileProvider\" Source File - {this.sourceFile.FileName}");

            var fileName = await this.outputFileNameProvider.ResolveValueIfNotNull(providerContext);

            this.flattenDataObject = this.flattenDataObjectProvider != null
                ? (await this.flattenDataObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                : true;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Flatten Data Object - {this.flattenDataObject}");

            this.repeatingRangeStartIndex = this.repeatingRangeStartIndexProvider != null
                ? (await this.repeatingRangeStartIndexProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                : 0;

            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Repeating Range Start Index - {this.flattenDataObject}");

            if (this.contentProviders != null)
            {
                foreach (var contentProvider in this.contentProviders)
                {
                    var cont = (await contentProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    if (cont.DataValue.Include)
                    {
                        this.content.Add(cont.DataValue);
                    }
                }
            }

            var environment = providerContext.AutomationData.System.Environment;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            this.removeUnusedMergeFields = this.removeUnusedMergeFieldsProvider != null
                ? (await this.removeUnusedMergeFieldsProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                : environment == DeploymentEnvironment.Production ? true : false;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Remove Unused Merge Fields - {this.removeUnusedMergeFieldsProvider}");

            this.removeRangesWhereAllMergeFieldsAreUnused
                = this.removeRangesWhereAllMergeFieldsAreUnusedProvider != null
                    ? (await this.removeRangesWhereAllMergeFieldsAreUnusedProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                    : false;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Remove Unused Ranges - {this.removeRangesWhereAllMergeFieldsAreUnused}");

            this.removeTablesWhereAllMergeFieldsAreUnused
                = this.removeTablesWhereAllMergeFieldsAreUnusedProvider != null
                    ? (await this.removeTablesWhereAllMergeFieldsAreUnusedProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                    : false;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Remove Unused Tables - {this.removeTablesWhereAllMergeFieldsAreUnused}");

            this.removeTableRowsWhereAllMergeFieldsAreUnused
                = this.removeTableRowsWhereAllMergeFieldsAreUnusedProvider != null
                    ? (await this.removeTableRowsWhereAllMergeFieldsAreUnusedProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                    : false;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Remove Unused Table Rows - {this.removeTableRowsWhereAllMergeFieldsAreUnused}");

            this.removeParagraphsWhereAllMergeFieldsAreUnused
                = this.removeParagraphsWhereAllMergeFieldsAreUnusedProvider != null
                    ? (await this.removeParagraphsWhereAllMergeFieldsAreUnusedProvider.Resolve(providerContext)).GetValueOrThrowIfFailed()
                    : environment == DeploymentEnvironment.Production ? true : false;
            this.logger.LogInformation(
                $"\"MSWordFileProvider\" Remove Unused Paragraphs - {this.removeParagraphsWhereAllMergeFieldsAreUnused}");

            this.dataObject = await this.CreateDataObject();

            this.outputFileName = string.IsNullOrEmpty(fileName?.ToString())
                ? this.sourceFile.FileName.ToString()
                : fileName?.ToString();
            this.logger.LogInformation($"\"MSWordFileProvider\" Output File - {this.outputFileName}");

            var outputFileNameInvalidVerificationResult = FileName.ValidateFileName(this.outputFileName);
            if (outputFileNameInvalidVerificationResult.IsFailure)
            {
                await this.GenerateExceptionOnFailedValidation(
                     outputFileNameInvalidVerificationResult,
                     (errorDetails, errorData) => Errors.File.FileNameInvalid(errorDetails, errorData));
            }

            await this.ValidateFileTypesUsed();

            byte[] outputBytes = await this.GenerateDocument(tenantId, this.outputFileName);
            return ProviderResult<Data<FileInfo>>.Success(new Data<FileInfo>(new FileInfo(this.outputFileName, outputBytes)));
        }

        private async Task GenerateExceptionOnFailedValidation(
            Result validationResults, Func<List<string>, JObject, Error> customException)
        {
            var errorData = await this.providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add("sourceFileName", this.sourceFile.FileName.ToString());
            errorData.Add("outputFileName", this.outputFileName);
            var additionalDetails = new List<string>
            {
                validationResults.Error,
            };
            throw new ErrorException(customException(additionalDetails, errorData));
        }

        private IMsWordFileFormat MsWordEngineSaveFormat()
        {
            string resolvedFileExtension = System.IO.Path.GetExtension(this.outputFileName);
            return new DocumentMsWordFileFormat(resolvedFileExtension);
        }

        private async Task<JObject> CreateDataObject()
        {
            JObject dataObject;
            if (this.dataObjectProvider == null)
            {
                dataObject = JObject.FromObject(this.providerContext!.AutomationData);
            }
            else
            {
                var data = (await this.dataObjectProvider.Resolve(this.providerContext!)).GetValueOrThrowIfFailed();
                dataObject = JObject.FromObject(data.GetValueFromGeneric());
            }

            IJsonObjectParser objectParser = new GenericJObjectParser(
                string.Empty, dataObject,
                this.flattenDataObject,
                Convert.ToInt32(this.repeatingRangeStartIndex));
            return objectParser.JsonObject ?? new JObject();
        }

        private List<string>? GetSupportedExtensionForOutputFileNameBasedOnSourceFileExtension(string extension)
        {
            List<string>? supportedExtensions = null;
            switch (extension)
            {
                case ".doc":
                case ".dot":
                    return new List<string>() { ".doc", ".dot" };
                case ".docx":
                case ".dotx":
                    return new List<string>() { ".dotx", ".docx" };
                case ".docm":
                case ".dotm":
                    return new List<string> { ".docm", ".dotm" };
            }

            return supportedExtensions;
        }

        private async Task GenerateExceptionForUnsupportedFileName(
            List<string> validExtensions, Func<List<string>, JObject, Error> customException, string filetype)
        {
            var validExtensionsDescription = string.Join(",", validExtensions.Select(ve => $"'{ve}'"));
            await this.GenerateExceptionOnFailedValidation(
                Result.Failure<string>(
                $"Valid {filetype} filename extensions for {ProviderName} provider:{validExtensionsDescription}"),
                (errorDetails, errorData) => customException(errorDetails, errorData));
        }

        private async Task ValidateFileTypesUsed()
        {
            string outputExtension = System.IO.Path.GetExtension(this.outputFileName);
            var validExtensions = MsWordDocumentHelper.GetSupportedDocumentExtensionByThePlatform();

            if (!validExtensions.Contains(outputExtension))
            {
                await this.GenerateExceptionForUnsupportedFileName(
                    validExtensions,
                    (details, errorData) => Errors.Automation.OutputFileHasUnsupportedExtensionForTheCurrentProvider(
                        errorData, details, ProviderName),
                    "output");
            }

            var sourceExtension = this.sourceFile.FileName.ToString().Substring(this.sourceFile.FileName.ToString().LastIndexOf("."));
            if (!validExtensions.Contains(sourceExtension))
            {
                await this.GenerateExceptionForUnsupportedFileName(
                    validExtensions,
                    (details, errorData) => Errors.Automation.InputFileHasUnsupportedExtensionForTheCurrentProvider(
                        errorData, details, ProviderName),
                    "input");
            }

            var supportedExtensionsPerSourceFile =
                this.GetSupportedExtensionForOutputFileNameBasedOnSourceFileExtension(sourceExtension);

            if (supportedExtensionsPerSourceFile == null)
            {
                await this.GenerateExceptionOnFailedValidation(
                    Result.Failure<string>($"Unable to find the supported extension for '{sourceExtension}'"),
                    (errorDetails, errorData) => Errors.Automation.SupportedExtensionsAreNotDefinedForSourceFileName(
                        errorData, errorDetails, ProviderName));
            }

            if (supportedExtensionsPerSourceFile != null && supportedExtensionsPerSourceFile.Any()
                && !supportedExtensionsPerSourceFile.Contains(outputExtension))
            {
                var supportedExtensions = supportedExtensionsPerSourceFile
                    .GenerateDescriptionUsingSingleQuoteAndCommaSeparationForEachItem();
                await this.GenerateExceptionOnFailedValidation(
                    Result.Failure<string>(
                        $"Valid output filename extensions based on source filename extension: {supportedExtensions}"),
                    (errorDetails, errorData) =>
                    {
                        return Errors.Automation.OutputFileHasInvalidExtensionForItsInputFile(
                                 errorData, errorDetails);
                    });
            }
        }

        private async Task<byte[]> GenerateDocument(Guid tenantId, string outputFileName)
        {
            var templateSourceFile = this.sourceFile.FileName.ToString();
            try
            {
                return this.gemBoxMsWordEngineService.MergeDataToTemplate(
                    tenantId,
                    this.dataObject,
                    this.sourceFile.Content,
                    this.removeUnusedMergeFields,
                    this.removeRangesWhereAllMergeFieldsAreUnused,
                    this.removeTablesWhereAllMergeFieldsAreUnused,
                    this.removeTableRowsWhereAllMergeFieldsAreUnused,
                    this.removeParagraphsWhereAllMergeFieldsAreUnused,
                    this.content);
            }
            catch (ErrorException ex)
            {
                var errorData = await this.providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add("outputFileName", outputFileName);
                errorData.Add("templateSourceFile", templateSourceFile);
                var additionalDetails = GenericErrorDataHelper.GenerateAdditionalDetailList(errorData);
                ex.EnrichAndRethrow(errorData, additionalDetails);

                throw;
            }
        }
    }
}
