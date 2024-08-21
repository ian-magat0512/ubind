// <copyright file="MsExcelFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Model for creating an instance of <see cref="MsExcelFileProvider"/>.
    /// </summary>
    public class MsExcelFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        /// <summary>
        /// Gets or sets the filename of the new file if defined. Otherwise, the filename of the source file is used.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the provider of the source file to be merged data into.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the data object that will be used to generate merge fields for the word file to merge with.
        /// If ommitted, the entire automation data will be used to generate merge field values.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            if (this.SourceFile == null)
            {
                var errorData = new JObject()
                {
                    { ErrorDataKey.ErrorMessage, "SourceFile property is missing." },
                };
                throw new ErrorException(
                    Errors.Automation.InvalidAutomationConfiguration(errorData));
            }

            var engineService = dependencyProvider.GetService<IMsExcelEngineService>();
            return new MsExcelFileProvider(
                this.OutputFileName?.Build(dependencyProvider),
                this.SourceFile.Build(dependencyProvider),
                this.DataObject?.Build(dependencyProvider),
                engineService);
        }
    }
}
