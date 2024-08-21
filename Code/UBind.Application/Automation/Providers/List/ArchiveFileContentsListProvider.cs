// <copyright file="ArchiveFileContentsListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.File.Model;

    /// <summary>
    /// A provider which resolves a list of files and their details from a compressed archive (e.g a zip file).
    /// </summary>
    public class ArchiveFileContentsListProvider : IDataListProvider<object>
    {
        private readonly IProvider<Data<FileInfo>> sourceFileProvider;
        private readonly IProvider<Data<string>>? passwordProvider;
        private readonly IProvider<Data<string>>? formatProvider;

        private FileInfo sourceFile = null!;
        private string? password;
        private string? format;
        private IClock clock;

        public ArchiveFileContentsListProvider(
            IProvider<Data<FileInfo>> sourceFileProvider,
            IProvider<Data<string>>? passwordProvider,
            IProvider<Data<string>>? formatProvider,
            IClock clock)
        {
            this.sourceFileProvider = sourceFileProvider;
            this.passwordProvider = passwordProvider;
            this.formatProvider = formatProvider;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public List<string> IncludedProperties { get; set; } = new List<string>();

        public string SchemaReferenceKey => "archiveFileContentsList";

        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            this.sourceFile = (await this.sourceFileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            this.password = (await this.passwordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.format = (await this.formatProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            var archive = await ArchiveFactory.Open(
                this.sourceFile,
                this.password,
                this.format,
                this.clock,
                async () => await this.GetErrorData(providerContext));
            using (archive)
            {
                List<ArchiveEntryModel> archiveEntries = new List<ArchiveEntryModel>();
                foreach (IArchiveEntry archiveEntry in archive)
                {
                    // we need to convert the archive entries to a non resource linked object since we will dispose of the archive.
                    archiveEntries.Add(new ArchiveEntryModel(archiveEntry));
                }

                return ProviderResult<IDataList<object>>.Success(new GenericDataList<object>(archiveEntries));
            }
        }

        private async Task<JObject> GetErrorData(IProviderContext providerContext)
        {
            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add("sourceFile", this.sourceFile.FileName.ToString());
            if (!string.IsNullOrEmpty(this.format))
            {
                errorData.Add("format", this.format);
            }

            return errorData;
        }
    }
}
