// <copyright file="CopyFileOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class CopyFileOperationConfigModel : OperationConfigModel, IBuilder<CopyFileOperation>
    {
        /// <summary>
        /// Gets or sets the location of the file that should be copied to the destination location.
        /// E.g. "source/folder/filename.ext".
        /// </summary>
        public IBuilder<IProvider<Data<string>>> SourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the destination location that the file should be copied to.
        /// E.g. "destination/folder/".
        /// If omitted, the file will be copied to the root of the archive.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the resulting file in the archive.
        /// If this property is used, the source file will automatically be renamed to the filename specified here
        /// before being copied to the destination folder.
        /// If omitted, the filename will not be changed.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the file identified by the from path cannot be found.
        /// Options are:
        /// 1. skip - the operation will be aborted but remaining operations will still be performed
        /// 2. end - the operation will be aborted and no further operations will be performed
        /// 3. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenSourceFileNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the folder identified by the to path cannot be
        /// found. Options are:
        /// 1. create - the missing folder will be created (Default)
        /// 2. skip - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if there is already a file or folder with the same name
        /// in the destination folder. Options are:
        /// 1. replace - the existing file/folder will be replaced
        /// 2. skip - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsEntryWithSameName { get; set; }

        public CopyFileOperation Build(IServiceProvider dependencyProvider)
        {
            return new CopyFileOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.SourceFilePath.Build(dependencyProvider),
                this.DestinationFolderPath?.Build(dependencyProvider),
                this.DestinationFileName?.Build(dependencyProvider),
                this.WhenSourceFileNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsEntryWithSameName?.Build(dependencyProvider));
        }
    }
}
