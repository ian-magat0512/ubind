// <copyright file="MoveFolderOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.File.Model;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ValueTypes;

    public class MoveFolderOperation : Operation
    {
        private readonly IProvider<Data<string>> sourceFolderPath;
        private readonly IProvider<Data<string>> destinationFolderPath;
        private readonly IProvider<Data<string>> destinationFolderName;
        private readonly IProvider<Data<string>> whenSourceFolderNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsFileWithSameName;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsFolderWithSameName;

        public MoveFolderOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> sourceFolderPath,
            IProvider<Data<string>> destinationFolderParentPath,
            IProvider<Data<string>> destinationFolderName,
            IProvider<Data<string>> whenSourceFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderParentNotFound,
            IProvider<Data<string>> whenDestinationFileExists,
            IProvider<Data<string>> whenDestinationFolderAlreadyExists)
            : base(runCondition)
        {
            this.sourceFolderPath = sourceFolderPath;
            this.destinationFolderPath = destinationFolderParentPath;
            this.destinationFolderName = destinationFolderName;
            this.whenSourceFolderNotFound = whenSourceFolderNotFound;
            this.whenDestinationFolderNotFound = whenDestinationFolderParentNotFound;
            this.whenDestinationFolderContainsFileWithSameName = whenDestinationFileExists;
            this.whenDestinationFolderContainsFolderWithSameName = whenDestinationFolderAlreadyExists;
        }

        protected enum FolderAlreadyExistsDecision
        {
            /// <summary>
            /// The existing folder will be replaced
            /// </summary>
            Replace,

            /// <summary>
            /// The entries will be added to the existing folder
            /// </summary>
            Merge,

            /// <summary>
            /// The operation will be aborted  but remaining operations will still be performed
            /// </summary>
            Skip,

            /// <summary>
            /// The operation will be aborted and no further operations will be performed
            /// </summary>
            End,

            /// <summary>
            /// The operation will be aborted and an error will be raised (Default)
            /// </summary>
            RaiseError,
        }

        protected virtual string OperationName => "move";

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            var path = (await this.sourceFolderPath.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var sourceFolderPath = new Path(path);
            string? destinationFolderName = (await this.destinationFolderName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var outputFolderName
                = new Path(destinationFolderName ?? sourceFolderPath.GetLastSegment());
            string? resolveDestinationFolderPath = (await this.destinationFolderPath.ResolveValueIfNotNull(providerContext))?.DataValue;
            var targetFolderPath = !string.IsNullOrEmpty(resolveDestinationFolderPath)
                ? new Path(resolveDestinationFolderPath).Join(outputFolderName)
                : outputFolderName;
            var destinationFolderPath = targetFolderPath.GetParent();

            // check if the source folder exists
            if (!archive.PathExists(sourceFolderPath))
            {
                var directive = await this.HandleSourceFolderNotFound(sourceFolderPath, archive, providerContext, getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            // check if the destination folder exists
            if (!string.IsNullOrEmpty(destinationFolderPath) && !archive.PathExists(destinationFolderPath))
            {
                var directive = await this.HandleDestinationFolderNotFound(
                    destinationFolderPath,
                    archive,
                    providerContext,
                    getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            // check if a file/folder exists at the target path
            if (!string.IsNullOrEmpty(targetFolderPath) && archive.PathExists(targetFolderPath))
            {
                var existingEntry = archive.GetEntry(targetFolderPath);
                ExecutionDirective directive;
                if (existingEntry != null && existingEntry.IsFile)
                {
                    directive = await this.HandleFileExists(
                        existingEntry,
                        archive,
                        providerContext,
                        getErrorDataCallback);
                }
                else
                {
                    directive = await this.HandleFolderExists(
                        targetFolderPath,
                        archive,
                        providerContext,
                        getErrorDataCallback);
                }

                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            this.CompleteExecution(archive, sourceFolderPath, targetFolderPath);
            return ExecutionDirective.Next;
        }

        protected virtual void CompleteExecution(
            IArchive archive,
            string sourceFolderPath,
            string targetFolderPath)
        {
            archive.MoveFolder(sourceFolderPath, targetFolderPath);
        }

        private async Task<ExecutionDirective> HandleSourceFolderNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenSourceFolderNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
            Decision decision = decisionString?.ToEnumOrThrow<Decision>() ?? Decision.RaiseError;
            switch (decision)
            {
                case Decision.Skip:
                    return ExecutionDirective.Next;
                case Decision.End:
                    return ExecutionDirective.End;
                case Decision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFolderOperation.SourceFolderNotFound(
                        this.OperationName, path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(Decision)));
            }
        }

        private async Task<ExecutionDirective> HandleDestinationFolderNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
            FolderNotFoundDecision decision
                = decisionString?.ToEnumOrThrow<FolderNotFoundDecision>() ?? FolderNotFoundDecision.Create;
            switch (decision)
            {
                case FolderNotFoundDecision.Create:
                    archive.AddFolder(path);
                    return ExecutionDirective.Continue;
                case FolderNotFoundDecision.Skip:
                    return ExecutionDirective.Next;
                case FolderNotFoundDecision.End:
                    return ExecutionDirective.End;
                case FolderNotFoundDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFolderOperation.DestinationFolderParentNotFound(
                        this.OperationName, path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderNotFoundDecision)));
            }
        }

        private async Task<ExecutionDirective> HandleFileExists(
            IArchiveEntry existingEntry,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderContainsFileWithSameName.ResolveValueIfNotNull(providerContext))?.DataValue;
            EntryAlreadyExistsDecision decision
                = decisionString?.ToEnumOrThrow<EntryAlreadyExistsDecision>()
                    ?? EntryAlreadyExistsDecision.RaiseError;
            switch (decision)
            {
                case EntryAlreadyExistsDecision.Replace:
                    archive.RemoveEntry(existingEntry);
                    return ExecutionDirective.Continue;
                case EntryAlreadyExistsDecision.Skip:
                    return ExecutionDirective.Next;
                case EntryAlreadyExistsDecision.End:
                    return ExecutionDirective.End;
                case EntryAlreadyExistsDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFolderOperation.DestinationFileExists(
                        this.OperationName, existingEntry.Path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(EntryAlreadyExistsDecision)));
            }
        }

        private async Task<ExecutionDirective> HandleFolderExists(
            string targetFolderPath,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderContainsFolderWithSameName.ResolveValueIfNotNull(providerContext))?.DataValue;
            FolderAlreadyExistsDecision decision
                = decisionString?.ToEnumOrThrow<FolderAlreadyExistsDecision>()
                    ?? FolderAlreadyExistsDecision.RaiseError;
            switch (decision)
            {
                case FolderAlreadyExistsDecision.Replace:
                    archive.RemoveEntries(targetFolderPath);
                    return ExecutionDirective.Continue;
                case FolderAlreadyExistsDecision.Merge:
                    return ExecutionDirective.Continue;
                case FolderAlreadyExistsDecision.Skip:
                    return ExecutionDirective.Next;
                case FolderAlreadyExistsDecision.End:
                    return ExecutionDirective.End;
                case FolderAlreadyExistsDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFolderOperation.DestinationFolderExists(
                        this.OperationName, targetFolderPath, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderAlreadyExistsDecision)));
            }
        }
    }
}
