// <copyright file="Operation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File.ArchiveFile
{
    using System;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File.Model;
    using UBind.Domain.Extensions;

    public abstract class Operation
    {
        public Operation(IProvider<Data<bool>>? runCondition)
        {
            this.RunCondition = runCondition;
        }

        public enum ExecutionDirective
        {
            /// <summary>
            /// Continue execution of the current operation
            /// </summary>
            Continue,

            /// <summary>
            /// Go to execution of the next operation.
            /// </summary>
            Next,

            /// <summary>
            /// Stop execution of the current and future operations
            /// </summary>
            End,
        }

        protected enum Decision
        {
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

        protected enum FolderNotFoundDecision
        {
            /// <summary>
            /// The missing folder will be created (Default)
            /// </summary>
            Create,

            /// <summary>
            /// The operation will be aborted  but remaining operations will still be performed
            /// </summary>
            Skip,

            /// <summary>
            /// The operation will be aborted and no further operations will be performed
            /// </summary>
            End,

            /// <summary>
            /// The operation will be aborted and an error will be raised
            /// </summary>
            RaiseError,
        }

        protected enum EntryAlreadyExistsDecision
        {
            /// <summary>
            /// The existing file will be replaced
            /// </summary>
            Replace,

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

        /// <summary>
        /// Gets the condition to be evaluated, if any, before running the action.
        /// </summary>
        public IProvider<Data<bool>>? RunCondition { get; }

        /// <summary>
        /// Performs the operation, returing a value indicating whether to continue with the next operation or not.
        /// </summary>
        /// <returns>A value indicating whether to continue with the next operation or not.</returns>
        public async Task<ExecutionDirective> Execute(IArchive archive, IProviderContext providerContext, Func<Task<JObject>> getErrorDataCallback)
        {
            var resolvedRunCondition = (await this.RunCondition.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (resolvedRunCondition != null && !resolvedRunCondition.Value)
            {
                return ExecutionDirective.Next;
            }

            return await this.DoExecute(archive, providerContext, getErrorDataCallback);
        }

        protected abstract Task<ExecutionDirective> DoExecute(
            IArchive archive,
            IProviderContext providerContext,
            Func<Task<JObject>> getErrorDataCallback);

        protected CompressionLevel ParseCompressionLevel(string? value)
        {
            // Until we upgrad our .NET version, CompressionLevel.SmallestSize doesn't exist yet.
            // See: https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.compressionlevel?view=net-6.0
            // And: https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.compressionlevel?view=netframework-4.8
            if (value == "smallestSize")
            {
                value = "optimal";
            }

            return value?.ToEnumOrThrow<CompressionLevel>() ?? CompressionLevel.Optimal;
        }
    }
}
