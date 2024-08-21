// <copyright file="AddFolderOperation.cs" company="uBind">
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

    public class AddFolderOperation : Operation
    {
        private readonly IProvider<Data<string>> folderName;
        private readonly IProvider<Data<string>> destinationFolderPath;
        private readonly IProvider<Data<string>> whenDestinationFolderNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsFileWithSameName;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsFolderWithSameName;

        public AddFolderOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> outputFolderName,
            IProvider<Data<string>> destinationFolderPath,
            IProvider<Data<string>> whenDestinationFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderContainsFileWithSameName,
            IProvider<Data<string>> whenDestinationFolderContainsFolderWithSameName)
            : base(runCondition)
        {
            this.folderName = outputFolderName;
            this.destinationFolderPath = destinationFolderPath;
            this.whenDestinationFolderNotFound = whenDestinationFolderNotFound;
            this.whenDestinationFolderContainsFileWithSameName = whenDestinationFolderContainsFileWithSameName;
            this.whenDestinationFolderContainsFolderWithSameName = whenDestinationFolderContainsFolderWithSameName;
        }

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            var resolveFolderName = (await this.folderName.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var outputFolderName = new Path(resolveFolderName);
            string? resolveDestinationFolderPath = (await this.destinationFolderPath.ResolveValueIfNotNull(providerContext))?.DataValue;
            var targetFolderPath = !string.IsNullOrEmpty(resolveDestinationFolderPath)
                ? new Path(resolveDestinationFolderPath).Join(outputFolderName)
                : outputFolderName;
            var destinationFolderPath = targetFolderPath.GetParent();

            // check if destination folder is not found
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

            // check if a file or folder exists at the target path
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

            archive.AddFolder(targetFolderPath);
            return ExecutionDirective.Next;
        }

        private async Task<ExecutionDirective> HandleFileExists(
            IArchiveEntry entry,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderContainsFileWithSameName.ResolveValueIfNotNull(providerContext))?.DataValue;
            EntryAlreadyExistsDecision decision
                = decisionString?.ToEnumOrThrow<EntryAlreadyExistsDecision>() ?? EntryAlreadyExistsDecision.RaiseError;
            switch (decision)
            {
                case EntryAlreadyExistsDecision.Replace:
                    archive.RemoveEntry(entry);
                    return ExecutionDirective.Continue;
                case EntryAlreadyExistsDecision.Skip:
                    return ExecutionDirective.Next;
                case EntryAlreadyExistsDecision.End:
                    return ExecutionDirective.End;
                case EntryAlreadyExistsDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.AddFolderOperation.EntryAlreadyExists(
                        entry.Path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(EntryAlreadyExistsDecision)));
            }
        }

        private async Task<ExecutionDirective> HandleFolderExists(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenDestinationFolderContainsFolderWithSameName.ResolveValueIfNotNull(providerContext))?.DataValue;
            EntryAlreadyExistsDecision decision
                = decisionString?.ToEnumOrThrow<EntryAlreadyExistsDecision>() ?? EntryAlreadyExistsDecision.Skip;
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
                    throw new ErrorException(Errors.Automation.Archive.AddFolderOperation.EntryAlreadyExists(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(EntryAlreadyExistsDecision)));
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
                    throw new ErrorException(Errors.Automation.Archive.AddFolderOperation.ParentFolderNotFound(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderNotFoundDecision)));
            }
        }
    }
}
