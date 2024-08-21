// <copyright file="RemoveEntryOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;

    public class RemoveEntryOperationConfigModel : OperationConfigModel, IBuilder<RemoveEntryOperation>
    {
        /// <summary>
        /// Gets or sets the path within the archive file that the entry to be deleted exists.
        /// E.g. "folder/name/filename.ext".
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what should happen if the entry identified by the path cannot be found.
        /// Options are:
        /// 1. continue - the operation will be aborted but remaining operations will still be performed
        /// 2. end - the operation will be aborted and no further operations will be performed
        /// 3. raiseError - the operation will be aborted and an error will be raised (Default).
        /// </summary>
        public IBuilder<IProvider<Data<string>>> WhenEntryNotFound { get; set; }

        public RemoveEntryOperation Build(IServiceProvider dependencyProvider)
        {
            return new RemoveEntryOperation(
                this.RunCondition?.Build(dependencyProvider),
                this.Path.Build(dependencyProvider),
                this.WhenEntryNotFound?.Build(dependencyProvider));
        }
    }
}
