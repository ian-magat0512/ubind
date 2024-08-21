// <copyright file="MoveFileOperation.cs" company="uBind">
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

    public class MoveFileOperation : Operation
    {
        private readonly IProvider<Data<string>> sourceFilePath;
        private readonly IProvider<Data<string>> destinationFolderPath;
        private readonly IProvider<Data<string>> destinationFileName;
        private readonly IProvider<Data<string>> whenSourceFileNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsEntryWithSameName;

        public MoveFileOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> sourceFilePath,
            IProvider<Data<string>> destinationFolderPath,
            IProvider<Data<string>> destinationFileName,
            IProvider<Data<string>> whenSourceFileNotFound,
            IProvider<Data<string>> whenDestinationFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderContainsEntryWithSameName)
            : base(runCondition)
        {
            this.sourceFilePath = sourceFilePath;
            this.destinationFolderPath = destinationFolderPath;
            this.destinationFileName = destinationFileName;
            this.whenSourceFileNotFound = whenSourceFileNotFound;
            this.whenDestinationFolderNotFound = whenDestinationFolderNotFound;
            this.whenDestinationFolderContainsEntryWithSameName = whenDestinationFolderContainsEntryWithSameName;
        }

        protected virtual string OperationName => "move";

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            // check if source file is found
            var resolvePath = (await this.sourceFilePath.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var sourceFilePath = new Path(resolvePath);
            var sourceArchiveEntry = archive.GetEntry(sourceFilePath);
            if (sourceArchiveEntry == null)
            {
                return await this.HandleSourceFileNotFound(sourceFilePath, archive, providerContext, getErrorDataCallback);
            }

            string? destinationFileName = (await this.destinationFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var outputFilename
                = new Path(destinationFileName ?? sourceFilePath.GetLastSegment());
            string? resolveDestinationFolderPath = (await this.destinationFolderPath.ResolveValueIfNotNull(providerContext))?.DataValue;
            var targetFilePath = !string.IsNullOrEmpty(resolveDestinationFolderPath)
                ? new Path(resolveDestinationFolderPath).Join(outputFilename)
                : outputFilename;
            var destinationFolderPath = targetFilePath.GetParent();

            // check if destination folder is not found
            if (!archive.PathExists(destinationFolderPath))
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

            // check if an entry exists at the target path
            if (archive.PathExists(targetFilePath))
            {
                var directive = await this.HandleDestinationEntryAlreadyExists(
                    targetFilePath,
                    archive,
                    providerContext,
                    getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            this.CompleteExecution(archive, sourceArchiveEntry, targetFilePath);
            return ExecutionDirective.Next;
        }

        protected virtual void CompleteExecution(
            IArchive archive,
            IArchiveEntry sourceArchiveEntry,
            string targetFilePath)
        {
            archive.MoveFile(sourceArchiveEntry, targetFilePath);
        }

        private async Task<ExecutionDirective> HandleSourceFileNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenSourceFileNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
            Decision decision = decisionString?.ToEnumOrThrow<Decision>() ?? Decision.RaiseError;
            switch (decision)
            {
                case Decision.Skip:
                    return ExecutionDirective.Next;
                case Decision.End:
                    return ExecutionDirective.End;
                case Decision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFileOperation.SourceFileNotFound(
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
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFileOperation.DestinationFolderNotFound(
                        this.OperationName, path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderNotFoundDecision)));
            }
        }

        private async Task<ExecutionDirective> HandleDestinationEntryAlreadyExists(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderContainsEntryWithSameName.ResolveValueIfNotNull(providerContext))?.DataValue;
            EntryAlreadyExistsDecision decision
                = decisionString?.ToEnumOrThrow<EntryAlreadyExistsDecision>() ?? EntryAlreadyExistsDecision.RaiseError;
            switch (decision)
            {
                case EntryAlreadyExistsDecision.Replace:
                    archive.RemoveEntries(path);
                    return ExecutionDirective.Continue;
                case EntryAlreadyExistsDecision.Skip:
                    return ExecutionDirective.Next;
                case EntryAlreadyExistsDecision.End:
                    return ExecutionDirective.End;
                case EntryAlreadyExistsDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.MoveOrCopyFileOperation.DestinationEntryAlreadyExists(
                        this.OperationName, path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(EntryAlreadyExistsDecision)));
            }
        }
    }
}
