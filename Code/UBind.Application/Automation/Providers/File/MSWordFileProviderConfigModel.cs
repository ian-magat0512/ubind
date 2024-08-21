// <copyright file="MSWordFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling.GemBoxServices;

    /// <summary>
    /// Model for creating an instance of <see cref="MSWordFileProvider"/>.
    /// </summary>
    public class MSWordFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
        /// <summary>
        /// Gets or sets the file name of the new file, if defined. Otherwise, the file name of the source file is used.
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

        /// <summary>
        /// Gets or sets the property that indicates whether to flatten the data object or not.
        /// If set to true or ommitted, all nested properties within the data object will be flattened into a single-level
        /// object structure in the view model for the merge process. For example,
        /// { "items" : [ { "property": "Sample" }, { "property": "Values"} ] } will turn into
        /// { "Items0Property": "Sample", "Items1Property": "Values" }
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> FlattenDataObject { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates the start index of arrays in the flattened data object
        /// For example, if set to 3, given this object { "items" : [ { "property": "Sample" }, { "property": "Values"} ] },
        /// the flattened result will be { "Items3Property": "Sample", "Items4Property": "Values" }
        /// If ommitted, the start index will be 1
        /// </summary>
        public IBuilder<IProvider<Data<long>>> RepeatingRangeStartIndex { get; set; }

        /// <summary>
        /// Gets or sets a list source files, each identified by an alias,
        /// whose content can be merged into the MS Word file using merge directives.
        /// </summary>
        public IEnumerable<ContentSourceFileProviderConfigModel> Content { get; set; }
            = Enumerable.Empty<ContentSourceFileProviderConfigModel>();

        /// <summary>
        /// Gets or sets the property that indicates whether to remove unused merge fields or not.
        /// If true, all Merge Fields that resolve to null or empty values will be removed from the document.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RemoveUnusedMergeFields { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates whether to remove unused ranges or not.
        /// If true, all ranges that contain Merge Fields and where all those Merge Fields resolve to
        /// null or empty values will be removed from the document.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RemoveRangesWhereAllMergeFieldsAreUnused { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates whether to remove unused tables or not.
        /// If true, all tables that contain Merge Fields and where all those Merge Fields resolve to
        /// null or empty values will be removed from the document.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RemoveTablesWhereAllMergeFieldsAreUnused { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates whether to remove unused table rows or not.
        /// If true, all table rows that contain Merge Fields and where all those Merge Fields resolve to
        /// null or empty values will be removed from the document.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RemoveTableRowsWhereAllMergeFieldsAreUnused { get; set; }

        /// <summary>
        /// Gets or sets the property that indicates whether to remove unused paragraphs or not.
        /// If true, all paragraphs that contain Merge Fields and where all those Merge Fields resolve
        /// to null or empty values will be removed from the document.
        /// </summary>
        public IBuilder<IProvider<Data<bool>>> RemoveParagraphsWhereAllMergeFieldsAreUnused { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            var msWordEngine = dependencyProvider.GetService<IGemBoxMsWordEngineService>();
            var logger = dependencyProvider.GetService<ILogger<MSWordFileProvider>>();
            return new MSWordFileProvider(
                this.OutputFileName?.Build(dependencyProvider),
                this.SourceFile.Build(dependencyProvider),
                this.DataObject?.Build(dependencyProvider),
                this.FlattenDataObject?.Build(dependencyProvider),
                this.RepeatingRangeStartIndex?.Build(dependencyProvider),
                this.Content?.Select(c => c.Build(dependencyProvider)),
                this.RemoveUnusedMergeFields?.Build(dependencyProvider),
                this.RemoveRangesWhereAllMergeFieldsAreUnused?.Build(dependencyProvider),
                this.RemoveTablesWhereAllMergeFieldsAreUnused?.Build(dependencyProvider),
                this.RemoveTableRowsWhereAllMergeFieldsAreUnused?.Build(dependencyProvider),
                this.RemoveParagraphsWhereAllMergeFieldsAreUnused?.Build(dependencyProvider),
                msWordEngine,
                logger);
        }
    }
}
