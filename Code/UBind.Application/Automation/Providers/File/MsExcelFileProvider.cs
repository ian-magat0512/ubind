// <copyright file="MsExcelFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// A file provider that replaces merge fields in an excel file
    /// with values from the automation data or a specified data object.
    /// </summary>
    public class MsExcelFileProvider : IProvider<Data<FileInfo>>, IMsExcelEngineDatasource
    {
        private readonly IProvider<Data<string>>? outputFileNameProvider;
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;
        private readonly IObjectProvider? dataObjectProvider;
        private readonly IMsExcelEngineService engineService;

        private IProviderContext? providerContext;
        private FileInfo sourceFile = null!;
        private string? fileName;
        private JObject? dataObject;
        private bool isDataObjectFormatted;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsExcelFileProvider"/> class.
        /// </summary>
        /// <param name="outputFileName">The name to be used for the generated file, if any.</param>
        /// <param name="sourceFile">The source file to be used to merge data on to.</param>
        /// <param name="dataObject">The data object to be used for generating the new file, if any.</param>
        /// <param name="engineService">The MS Excel engine service.</param>
        public MsExcelFileProvider(
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<FileInfo>> sourceFile,
            IObjectProvider? dataObject,
            IMsExcelEngineService engineService)
        {
            Contract.Assert(sourceFile != null);

            this.outputFileNameProvider = outputFileName;
            this.sourceFileProvider = sourceFile;
            this.dataObjectProvider = dataObject;
            this.engineService = engineService;
        }

        /// <inheritdoc/>
        public byte[] Content => this.sourceFile.Content;

        /// <inheritdoc/>
        public DeploymentEnvironment Environment
            => this.providerContext!.AutomationData.System.Environment;

        public string SchemaReferenceKey => "msExcelFile";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            this.providerContext = providerContext;
            this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            string? outputFileName = (await this.outputFileNameProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.fileName = string.IsNullOrEmpty(outputFileName)
                ? this.sourceFile.FileName.ToString()
                : outputFileName;

            await this.ValidateFileTypesUsed(providerContext);
            this.dataObject = await this.CreateDataObject();

            this.engineService.Datasource = this;
            byte[] outputBytes =
                this.engineService.GenerateContent(this.sourceFile.FileName.ToString(), this.fileName);

            return ProviderResult<Data<FileInfo>>.Success(new Data<FileInfo>(new FileInfo(this.fileName, outputBytes)));
        }

        /// <inheritdoc/>
        public JToken GetObject(string path)
        {
            if (!this.isDataObjectFormatted)
            {
                if (!PathHelper.IsJsonPointer(path))
                {
                    // PascalCase for JSON path
                    this.dataObject = (JObject)this.dataObject!.CapitalizePropertyNames();
                }

                this.isDataObjectFormatted = true;
            }

            return this.dataObject!.GetToken(path);
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
                var data = (await this.dataObjectProvider.Resolve(this.providerContext!)).GetValueOrThrowIfFailed().DataValue;
                dataObject = JObject.FromObject(data);
            }

            return dataObject;
        }

        private async Task ValidateFileTypesUsed(IProviderContext providerContext)
        {
            string[] validFileExtensions = { ".xlsx", ".xltx", ".xltm", ".xlsm", ".xls", ".xlt" };
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add("sourceFilename", this.sourceFile.FileName.ToString());
            errorData.Add("outputFilename", this.fileName);

            if (!FileName.HasValidFileExtension(this.sourceFile.FileName.ToString(), validFileExtensions))
            {
                errorData.Add("validExtensions", string.Join(", ", validFileExtensions));
                throw new ErrorException(Errors.Automation.ExcelProviderSourceFilenameError(errorData));
            }
            else if (!FileName.HasValidFileExtension(this.fileName!, validFileExtensions))
            {
                errorData.Add("validExtensions", string.Join(", ", validFileExtensions));
                throw new ErrorException(Errors.Automation.ExcelProviderOutputFilenameError(errorData));
            }
            else if (FileName.ValidateFileName(this.fileName!).IsFailure)
            {
                throw new ErrorException(Errors.File.FileNameInvalid(null, errorData));
            }
        }
    }
}
