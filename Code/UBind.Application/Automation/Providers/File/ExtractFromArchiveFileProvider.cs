// <copyright file="ExtractFromArchiveFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File.Model;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// A provider which provides a file by extracting one from an archive (e.g. a zip file).
    /// </summary>
    public class ExtractFromArchiveFileProvider : IProvider<Data<FileInfo>>
    {
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;
        private readonly IProvider<Data<string>>? passwordProvider;
        private readonly IProvider<Data<string>>? formatProvider;
        private readonly IProvider<Data<string>> filePathProvider;
        private readonly IProvider<Data<string>>? outputFilenameProvider;

        private FileInfo? sourceFile;
        private string? password;
        private string? format;
        private string? filePath;
        private string? outputFilename;
        private IClock clock;

        public ExtractFromArchiveFileProvider(
            IProvider<Data<FileInfo>> sourceFileProvider,
            IProvider<Data<string>>? passwordProvider,
            IProvider<Data<string>>? formatProvider,
            IProvider<Data<string>> filePathProvider,
            IProvider<Data<string>>? outputFilenameProvider,
            IClock clock)
        {
            this.sourceFileProvider = sourceFileProvider;
            this.passwordProvider = passwordProvider;
            this.formatProvider = formatProvider;
            this.filePathProvider = filePathProvider;
            this.outputFilenameProvider = outputFilenameProvider;
            this.clock = clock;
        }

        public string SchemaReferenceKey => "extractFromArchiveFile";

        public async ITask<IProviderResult<Data<FileInfo>>> Resolve(IProviderContext providerContext)
        {
            this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            this.password = (await this.passwordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.format = (await this.formatProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.filePath = (await this.filePathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            this.outputFilename = (await this.outputFilenameProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            var archive = await ArchiveFactory.Open(
                this.sourceFile,
                this.password,
                this.format,
                this.clock,
                async () => await this.GetErrorData(providerContext));
            using (archive)
            {
                var archiveEntry = archive.GetEntry(this.filePath);
                if (archiveEntry == null)
                {
                    var errorData = await this.GetErrorData(providerContext);
                    throw new ErrorException(Errors.Automation.Archive.EntryNotFound(this.filePath, errorData));
                }

                // prepare the output file name
                if (string.IsNullOrEmpty(this.outputFilename))
                {
                    this.outputFilename = System.IO.Path.GetFileName(this.filePath);
                }

                // read the archive entry
                try
                {
                    byte[] bytes = await archiveEntry.ReadBytes(async () => await this.GetErrorData(providerContext));
                    return ProviderResult<Data<FileInfo>>.Success(new FileInfo(this.outputFilename, bytes));
                }
                catch (System.IO.InvalidDataException)
                {
                    if (string.IsNullOrEmpty(this.password))
                    {
                        throw new ErrorException(Errors.Automation.Archive.NoPasswordSupplied(await this.GetErrorData(providerContext)));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
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

            errorData.Add("filePath", this.filePath);
            if (!string.IsNullOrEmpty(this.outputFilename))
            {
                errorData.Add("outputFilename", this.outputFilename);
            }

            return errorData;
        }
    }
}
