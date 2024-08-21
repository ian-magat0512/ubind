// <copyright file="MoveFileOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class MoveFileOperationConfigModel : CopyFileOperationConfigModel, IBuilder<MoveFileOperation>
    {
        MoveFileOperation IBuilder<MoveFileOperation>.Build(IServiceProvider dependencyProvider)
        {
            return new MoveFileOperation(
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
