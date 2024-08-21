// <copyright file="MoveFolderOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class MoveFolderOperationConfigModel : OperationConfigModel, IBuilder<MoveFolderOperation>
    {
        /// <summary>
        /// Gets or sets the location of the folder that should be moved to the destination location.
        /// E.g. "source/folder".
        /// </summary>
        public IBuilder<IProvider<Data<string>>> SourceFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the destination folder (parent) that the source folder should be moved to.
        /// E.g. "destination/folder/".
        /// If omitted, the folder will be moved to the root of the archive.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the destination folder in the archive.
        /// If this property is used, the source folder will automatically be renamed to the name specified here
        /// when it's placed in the destination folder.
        /// If omitted, the folder name will not be changed.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> DestinationFolderName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the folder identified by the from path cannot be found.
        /// Options are:
        /// 1. skip - the operation will be aborted but remaining operations will still be performed
        /// 2. end - the operation will be aborted and no further operations will be performed
        /// 3. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenSourceFolderNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the destination folder was not found.
        /// Options are:
        /// 1. create - the folder will be created (Default)
        /// 2. skip - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderNotFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if there is already a file with the same name as the
        /// destination folder. Options are:
        /// 1. replace - the existing file will be replaced with the folder (effectively removing the file first)
        /// 2. skip - the operation will be aborted but remaining operations will still be performed
        /// 3. end - the operation will be aborted and no further operations will be performed
        /// 4. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsFileWithSameName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if there is already a folder with the same name as the
        /// destination folder. Options are:
        /// 1. replace - the existing folder will be replaced with the folder (effectively removing the existing first)
        /// 2. merge - the folder to be moved is merged with the existing folder, overwriting any files with the same name.
        /// 3. skip - the operation will be aborted but remaining operations will still be performed
        /// 4. end - the operation will be aborted and no further operations will be performed
        /// 5. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsFolderWithSameName { get; set; }

        public MoveFolderOperation Build(IServiceProvider dependencyProvider)
        {
            return new MoveFolderOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.SourceFolderPath.Build(dependencyProvider),
                this.DestinationFolderPath?.Build(dependencyProvider),
                this.DestinationFolderName?.Build(dependencyProvider),
                this.WhenSourceFolderNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsFileWithSameName?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsFolderWithSameName?.Build(dependencyProvider));
        }
    }
}
