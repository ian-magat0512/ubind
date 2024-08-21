// <copyright file="ReplaceFileOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class ReplaceFileOperationConfigModel : OperationConfigModel, IBuilder<ReplaceFileOperation>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the builder of the provider of the file that should replace the target file in the archive.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the name of the file.
        /// If this property is used, the operation will try to replace an entry (file or folder) with this name.
        /// If omitted, the filename of the sourceFile will be used.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? TargetFileName { get; set; }

        /// <summary>
        /// Gets or sets the path of file should be replaced in the archive.
        /// E.g. "folder/name/".
        /// If omitted, the target file will be assumed to be in the root of the archive.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? TargetFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the compression level to use when compressing the file into the archive.
        /// One of:
        /// 1. optimal
        /// 2. fastest
        /// 3. noCompression
        /// 5. smallestSize.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? CompressionLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the target file is not found within the archive file.
        /// Options are:
        /// 1. add - the file will be added to the target location
        /// 2. continue - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? WhenTargetEntryNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the target file was not found when attempting to
        /// replace, and the 'add' behaviour was selected, but the folder identified by the path parameter doesn't
        /// exist. Options are:
        /// 1. create - the missing folder will be created (Default)
        /// 2. continue - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? WhenTargetFolderNotFound { get; set; }

        public ReplaceFileOperation Build(IServiceProvider dependencyProvider)
        {
            return new ReplaceFileOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.SourceFile.Build(dependencyProvider),
                this.TargetFileName?.Build(dependencyProvider),
                this.TargetFolderPath?.Build(dependencyProvider),
                this.CompressionLevel?.Build(dependencyProvider),
                this.WhenTargetEntryNotFound?.Build(dependencyProvider),
                this.WhenTargetFolderNotFound?.Build(dependencyProvider));
        }
    }
}
