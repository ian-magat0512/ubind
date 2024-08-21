// <copyright file="ReplaceFileOperation.cs" company="uBind">
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

    public class ReplaceFileOperation : Operation
    {
        private readonly IProvider<Data<FileInfo>> sourceFile;
        private readonly IProvider<Data<string>>? targetFileName;
        private readonly IProvider<Data<string>>? targetFolderPath;
        private readonly IProvider<Data<string>>? compressionLevel;
        private readonly IProvider<Data<string>>? whenTargetEntryNotFound;
        private readonly IProvider<Data<string>>? whenTargetFolderNotFound;

        private Path targetFilePath = null!;

        public ReplaceFileOperation(
            IProvider<Data<bool>>? runCondition,
            IProvider<Data<FileInfo>> sourceFile,
            IProvider<Data<string>>? targetFileName,
            IProvider<Data<string>>? targetFolderPath,
            IProvider<Data<string>>? compressionLevel,
            IProvider<Data<string>>? whenTargetEntryNotFound,
            IProvider<Data<string>>? whenTargetFolderNotFound)
            : base(runCondition)
        {
            this.sourceFile = sourceFile;
            this.targetFileName = targetFileName;
            this.targetFolderPath = targetFolderPath;
            this.compressionLevel = compressionLevel;
            this.whenTargetEntryNotFound = whenTargetEntryNotFound;
            this.whenTargetFolderNotFound = whenTargetFolderNotFound;
        }

        protected enum TargetEntryNotFoundDecision
        {
            /// <summary>
            /// The file will be added to the target location
            /// </summary>
            Add,

            /// <summary>
            /// The operation will be aborted but remaining operations will still be performed
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

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            FileInfo sourceFile = (await this.sourceFile.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            string? resolveTargetFolderPath = (await this.targetFolderPath.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? resolveTargetFileName = (await this.targetFileName.ResolveValueIfNotNull(providerContext))?.DataValue;
            var outputFilename
                = new Path(resolveTargetFileName ?? sourceFile.FileName.ToString());
            this.targetFilePath = !string.IsNullOrEmpty(resolveTargetFolderPath)
                ? new Path(resolveTargetFolderPath).Join(outputFilename)
                : outputFilename;

            var existingEntry = archive.GetEntry(this.targetFilePath);
            if (existingEntry == null)
            {
                var directive = await this.HandleTargetEntryNotFound(this.targetFilePath, archive, providerContext, getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }
            else
            {
                archive.RemoveEntries(this.targetFilePath);
            }

            string? compressionLevelString = (await this.compressionLevel.ResolveValueIfNotNull(providerContext))?.DataValue;
            var compressionLevel = this.ParseCompressionLevel(compressionLevelString);
            archive.AddFile(
                this.targetFilePath,
                sourceFile.Content,
                sourceFile.CreatedTimestamp,
                sourceFile.LastModifiedTimestamp,
                compressionLevel);
            return ExecutionDirective.Next;
        }

        private async Task<ExecutionDirective> HandleTargetEntryNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenTargetEntryNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
            TargetEntryNotFoundDecision decision
                = decisionString?.ToEnumOrThrow<TargetEntryNotFoundDecision>() ?? TargetEntryNotFoundDecision.RaiseError;
            switch (decision)
            {
                case TargetEntryNotFoundDecision.Add:
                    var folderPath = new Path(path).GetParent();
                    if (!folderPath.IsEmpty())
                    {
                        var folderEntry = archive.GetEntry(folderPath);
                        if (folderEntry == null)
                        {
                            return await this.HandleFolderNotFound(folderPath, archive, providerContext, getErrorDataCallback);
                        }
                    }

                    return ExecutionDirective.Continue;
                case TargetEntryNotFoundDecision.Skip:
                    return ExecutionDirective.Next;
                case TargetEntryNotFoundDecision.End:
                    return ExecutionDirective.End;
                case TargetEntryNotFoundDecision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.ReplaceFileOperation.FileNotFound(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(Decision)));
            }
        }

        private async Task<ExecutionDirective> HandleFolderNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenTargetFolderNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
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
                    throw new ErrorException(Errors.Automation.Archive.ReplaceFileOperation.FolderNotFound(
                        this.targetFilePath, path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(FolderNotFoundDecision)));
            }
        }
    }
}
