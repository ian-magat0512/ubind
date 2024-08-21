// <copyright file="AddFileOperation.cs" company="uBind">
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

    public class AddFileOperation : Operation
    {
        private readonly IProvider<Data<FileInfo>> sourceFile;
        private readonly IProvider<Data<string>> destinationFileName;
        private readonly IProvider<Data<string>> destinationFolderPath;
        private readonly IProvider<Data<string>> compressionLevel;
        private readonly IProvider<Data<string>> whenDestinationFolderNotFound;
        private readonly IProvider<Data<string>> whenDestinationFolderContainsEntryWithSameName;

        public AddFileOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<FileInfo>> sourceFile,
            IProvider<Data<string>> outputFilename,
            IProvider<Data<string>> destinationFolderPath,
            IProvider<Data<string>> compressionLevel,
            IProvider<Data<string>> whenDestinationFolderNotFound,
            IProvider<Data<string>> whenDestinationFolderContainsEntryWithSameName)
            : base(runCondition)
        {
            this.sourceFile = sourceFile;
            this.destinationFileName = outputFilename;
            this.destinationFolderPath = destinationFolderPath;
            this.compressionLevel = compressionLevel;
            this.whenDestinationFolderNotFound = whenDestinationFolderNotFound;
            this.whenDestinationFolderContainsEntryWithSameName = whenDestinationFolderContainsEntryWithSameName;
        }

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            FileInfo fileInfo = (await this.sourceFile.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var destinationFileName = (await this.destinationFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var outputFilename
                = new Path(destinationFileName ?? fileInfo.FileName.ToString());
            var resolveDestinationFolderPath = (await this.destinationFolderPath.ResolveValueIfNotNull(providerContext))?.DataValue;
            var destinationFolderPath = resolveDestinationFolderPath != null
                ? new Path(resolveDestinationFolderPath)
                : null;
            var targetFilePath = destinationFolderPath != null
                ? new Path(destinationFolderPath).Join(outputFilename)
                : outputFilename;
            destinationFolderPath = targetFilePath.GetParent();

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

            // check if an entry exists at the target path
            if (archive.PathExists(targetFilePath))
            {
                var directive = await this.HandleEntryAlreadyExists(
                    targetFilePath,
                    archive,
                    providerContext,
                    getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            string? compressionLevelString = (await this.compressionLevel.ResolveValueIfNotNull(providerContext))?.DataValue;
            var compressionLevel = this.ParseCompressionLevel(compressionLevelString);
            archive.AddFile(
                targetFilePath,
                fileInfo.Content,
                fileInfo.CreatedTimestamp,
                fileInfo.LastModifiedTimestamp,
                compressionLevel);
            return ExecutionDirective.Next;
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
                    throw new ErrorException(Errors.Automation.Archive.AddFileOperation.DestinationFolderNotFound(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderNotFoundDecision)));
            }
        }

        private async Task<ExecutionDirective> HandleEntryAlreadyExists(
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
                    throw new ErrorException(Errors.Automation.Archive.AddFileOperation.EntryAlreadyExists(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(EntryAlreadyExistsDecision)));
            }
        }
    }
}
