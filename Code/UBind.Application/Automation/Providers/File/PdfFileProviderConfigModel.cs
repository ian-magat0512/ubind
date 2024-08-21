// <copyright file="PdfFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FileHandling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Model for creating an instance of <see cref="PdfFileProvider"/>.
    /// </summary>
    public class PdfFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>, IOutputFileNameConfigModel, ISourceFileConfigModel
    {
        /// <inheritdoc/>
        public IBuilder<IProvider<Data<string>>>? OutputFileName { get; set; }

        /// <inheritdoc/>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            var pdfEngineService = dependencyProvider.GetService<IPdfEngineService>();

            if (this.SourceFile == null)
            {
                var errorData = new JObject
                {
                    { "sourceFile", "Required when other file providers is not defined (as long as that provider is already supported)" },
                    { "outputFileName", "Optional" },
                    { "others", "Please check the json automations schema" },
                };

                var details = new List<string>
                {
                    "Neither the sourceFile or one of these sources binaryFile, msWordFile, textFile, productFile, "
                    + "pdfFile, msExcelFile, entityFile, zipFile, objectPathLookupFile (as long as it is already "
                    + "supported by the platform) are defined in the schema.",
                };

                throw new ErrorException(Errors.Automation.PdfSourcesNotFound(details, errorData));
            }

            var logger = dependencyProvider.GetService<ILogger<PdfFileProvider>>();

            return new PdfFileProvider(
                    this.OutputFileName?.Build(dependencyProvider),
                    this.SourceFile.Build(dependencyProvider),
                    pdfEngineService,
                    logger);
        }
    }
}
