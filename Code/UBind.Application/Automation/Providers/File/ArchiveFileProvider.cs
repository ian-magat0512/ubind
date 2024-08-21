// <copyright file="ArchiveFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File.Model;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class ArchiveFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly IProvider<Data<FileInfo>>? sourceFileProvider;
        private readonly IProvider<Data<string>>? sourceFilePasswordProvider;
        private readonly IProvider<Data<string>>? formatProvider;
        private readonly IProvider<Data<string>>? outputFileNameProvider;
        private readonly IProvider<Data<string>>? passwordProvider;
        private readonly IEnumerable<ArchiveFile.Operation> operations;
        private readonly IClock clock;

        private FileInfo? sourceFile;
        private string? sourceFilePassword;
        private string? format;
        private string? outputFilename;
        private string? password;

        public ArchiveFileProvider(
            IProvider<Data<FileInfo>>? sourceFile,
            IProvider<Data<string>>? sourceFilePassword,
            IProvider<Data<string>>? format,
            IProvider<Data<string>>? outputFileName,
            IProvider<Data<string>>? password,
            IEnumerable<ArchiveFile.Operation> operations,
            IClock clock)
        {
            this.sourceFileProvider = sourceFile;
            this.sourceFilePasswordProvider = sourceFilePassword;
            this.formatProvider = format;
            this.outputFileNameProvider = outputFileName;
            this.passwordProvider = password;
            this.operations = operations;
            this.clock = clock;
        }

        public string SchemaReferenceKey => "archiveFile";

        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            this.format = (await this.formatProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? "zip";
            this.sourceFilePassword = (await this.sourceFilePasswordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.outputFilename = (await this.outputFileNameProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            IArchive archive;
            if (this.sourceFileProvider != null)
            {
                // Open the existing archive
                this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                archive = await ArchiveFactory.Open(
                    this.sourceFile,
                    this.sourceFilePassword,
                    this.format,
                    this.clock,
                    async () => await this.GetErrorData(providerContext));
                this.outputFilename = this.outputFilename ?? this.sourceFile.FileName.ToString();
            }
            else
            {
                if (this.outputFilename == null)
                {
                    throw new ErrorException(
                        Errors.Automation.Archive.FilenameRequiredWhenCreatingNewArchive(
                            await this.GetErrorData(providerContext)));
                }

                if (string.IsNullOrEmpty(this.format) && this.outputFilename.EndsWith(".zip"))
                {
                    this.format = "zip";
                }

                // Create a new, empty archive
                this.password = (await this.passwordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                archive = await ArchiveFactory.Create(
                    this.format,
                    this.password,
                    this.clock,
                    async () => await this.GetErrorData(providerContext));
            }

            using (archive)
            {
                // Execute each of the operations on the archive
                if (this.operations != null && this.operations.Any())
                {
                    foreach (var operation in this.operations)
                    {
                        var executionDirective = await operation.Execute(
                            archive,
                            providerContext,
                            async () => await this.GetErrorData(providerContext));
                        if (executionDirective == ArchiveFile.Operation.ExecutionDirective.End)
                        {
                            break;
                        }
                    }
                }
            }

            // Return the archive as a file
            return ProviderResult<Data<FileInfo>>.Success(new FileInfo(
                this.outputFilename,
                archive.ReadBytes(async () => await this.GetErrorData(providerContext)),
                archive.CreatedTimestamp,
                archive.LastModifiedTimestamp));
        }

        private async Task<JObject> GetErrorData(IProviderContext providerContext)
        {
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (this.sourceFile != null)
            {
                errorData.Add("sourceFile", this.sourceFile.FileName.ToString());
            }

            if (!string.IsNullOrEmpty(this.format))
            {
                errorData.Add("format", this.format);
            }

            if (!string.IsNullOrEmpty(this.outputFilename))
            {
                errorData.Add("outputFilename", this.outputFilename);
            }

            return errorData;
        }
    }
}
