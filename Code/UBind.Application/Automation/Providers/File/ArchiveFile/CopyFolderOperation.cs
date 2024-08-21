// <copyright file="CopyFolderOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using UBind.Application.Automation.Providers.File.Model;

    public class CopyFolderOperation : MoveFolderOperation
    {
        public CopyFolderOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> sourceFolderPath,
            IProvider<Data<string>> destinationFolderParentPath,
            IProvider<Data<string>> outputFolderName,
            IProvider<Data<string>> whenSourceFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderContainsFileWithSameName,
            IProvider<Data<string>> whenDestinationFolderContainsFolderWithSameName)
            : base(
                runCondition,
                sourceFolderPath,
                destinationFolderParentPath,
                outputFolderName,
                whenSourceFolderNotFound,
                whenDestinationFolderNotFound,
                whenDestinationFolderContainsFileWithSameName,
                whenDestinationFolderContainsFolderWithSameName)
        {
        }

        protected override string OperationName => "copy";

        protected override void CompleteExecution(
            IArchive archive,
            string sourceFolderPath,
            string targetFolderPath)
        {
            archive.CopyFolder(sourceFolderPath, targetFolderPath);
        }
    }
}
