// <copyright file="AddFileOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class AddFileOperationConfigModel : OperationConfigModel, IBuilder<AddFileOperation>
    {
        /// <summary>
        /// Gets or sets the builder of the provider of the file to be added to the archive.
        /// </summary>
        public IBuilder<IProvider<Data<FileInfo>>> SourceFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// If this property is used, the source file will automatically be renamed to the filename specified here
        /// using a text provider before being added to the archive file. If omitted, the filename of the sourceFile
        /// will be used.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFileName { get; set; }

        /// <summary>
        /// Gets or sets the path at which the file should be added to the archive.
        /// The location within the archive file that the new file should be added to. E.g. "folder/name/".
        /// If omitted, the file will be added to the root of the archive.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the compression level to use when compressing the file into the archive.
        /// One of:
        /// 1. optimal
        /// 2. fastest
        /// 3. noCompression
        /// 5. smallestSize.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> CompressionLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the folder identified by the path parameter doesn't
        /// exist. Options are:
        /// 1. create - the missing folder will be created (Default)
        /// 2. continue - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if a file with the same filename alerady exists at the
        /// location specified by the path parameter. Options are:
        /// 1. replace - the existing file will be replaced
        /// 2. continue - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsEntryWithSameName { get; set; }

        public AddFileOperation Build(IServiceProvider dependencyProvider)
        {
            return new AddFileOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.SourceFile.Build(dependencyProvider),
                this.DestinationFileName?.Build(dependencyProvider),
                this.DestinationFolderPath?.Build(dependencyProvider),
                this.CompressionLevel?.Build(dependencyProvider),
                this.WhenDestinationFolderNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsEntryWithSameName?.Build(dependencyProvider));
        }
    }
}
