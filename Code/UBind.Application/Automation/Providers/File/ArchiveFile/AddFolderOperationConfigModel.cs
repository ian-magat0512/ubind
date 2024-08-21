// <copyright file="AddFolderOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class AddFolderOperationConfigModel : OperationConfigModel, IBuilder<AddFolderOperation>
    {
        public IBuilder<IProvider<Data<string>>> FolderName { get; set; }

        public IBuilder<IProvider<Data<string>>> DestinationFolderPath { get; set; }

        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderNotFound { get; set; }

        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsFileWithSameName { get; set; }

        public IBuilder<IProvider<Data<string>>> WhenDestinationFolderContainsFolderWithSameName { get; set; }

        public AddFolderOperation Build(IServiceProvider dependencyProvider)
        {
            return new AddFolderOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.FolderName.Build(dependencyProvider),
                this.DestinationFolderPath?.Build(dependencyProvider),
                this.WhenDestinationFolderNotFound?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsFileWithSameName?.Build(dependencyProvider),
                this.WhenDestinationFolderContainsFolderWithSameName?.Build(dependencyProvider));
        }
    }
}
