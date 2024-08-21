// <copyright file="CopyFileOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using UBind.Application.Automation.Providers.File.Model;

    public class CopyFileOperation : MoveFileOperation
    {
        public CopyFileOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> sourceFilePath,
            IProvider<Data<string>> destinationFolderPath,
            IProvider<Data<string>> destinationFileName,
            IProvider<Data<string>> whenSourceFileNotFound,
            IProvider<Data<string>> whenDestinationFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderContainsEntryWithSameName)
            : base(
                runCondition,
                sourceFilePath,
                destinationFolderPath,
                destinationFileName,
                whenSourceFileNotFound,
                whenDestinationFolderNotFound,
                whenDestinationFolderContainsEntryWithSameName)
        {
        }

        protected override string OperationName => "copy";

        protected override void CompleteExecution(
            IArchive archive,
            IArchiveEntry sourceArchiveEntry,
            string destinationPath)
        {
            archive.CopyFile(sourceArchiveEntry, destinationPath);
        }
    }
}
