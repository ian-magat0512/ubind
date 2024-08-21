// <copyright file="RemoveEntryOperation.cs" company="uBind">
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

    public class RemoveEntryOperation : Operation
    {
        private readonly IProvider<Data<string>> path;
        private readonly IProvider<Data<string>> whenEntryNotFound;

        public RemoveEntryOperation(
            IProvider<Data<bool>> runCondition,
            IProvider<Data<string>> path,
            IProvider<Data<string>> whenEntryNotFound)
            : base(runCondition)
        {
            this.path = path;
            this.whenEntryNotFound = whenEntryNotFound;
        }

        protected override async Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            var resolvedPath = (await this.path.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var path = new Path(resolvedPath);

            // check if a file/folder exists at the target path
            if (!path.IsEmpty() && !archive.PathExists(path))
            {
                var directive = await this.HandleEntryNotFound(path, archive, providerContext, getErrorDataCallback);
                if (directive != ExecutionDirective.Continue)
                {
                    return directive;
                }
            }

            archive.RemoveEntries(path);
            return ExecutionDirective.Next;
        }

        private async Task<ExecutionDirective> HandleEntryNotFound(
            string path,
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback)
        {
            string? decisionString = (await this.whenEntryNotFound.ResolveValueIfNotNull(providerContext))?.DataValue;
            Decision decision = decisionString?.ToEnumOrThrow<Decision>() ?? Decision.RaiseError;
            switch (decision)
            {
                case Decision.Skip:
                    return ExecutionDirective.Next;
                case Decision.End:
                    return ExecutionDirective.End;
                case Decision.RaiseError:
                    throw new ErrorException(Errors.Automation.Archive.RemoveEntryOperation.EntryNotFound(
                        path, await getErrorDataCallback()));
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        decision, typeof(Decision)));
            }
        }
    }
}
