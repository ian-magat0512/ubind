// <copyright file="FixedObjectPathLookup.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup
{
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Reads a value from a the automation data using a path defined by a text provider.
    /// </summary>
    public class FixedObjectPathLookup : IObjectPathLookupProvider
    {
        private readonly IProvider<Data<string>> path;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedObjectPathLookup"/> class.
        /// </summary>
        /// <param name="path">The string path.</param>
        public FixedObjectPathLookup(IProvider<Data<string>> path, string schemaKey)
        {
            this.path = path;
            this.SchemaReferenceKey = schemaKey;
        }

        /// <summary>
        /// Gets the reference key for this provider.
        /// </summary>
        /// <remarks>Reference key to be used should be from the type-specific implementation.</remarks>
        public string SchemaReferenceKey { get; }

        /// <summary>
        /// Obtains a value from the automations data context via the given path.
        /// </summary>
        /// <returns>The object value.</returns>
        public async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(FixedObjectPathLookup) + "." + nameof(this.Resolve)))
            {
                var resolveDataPath = await this.path.Resolve(providerContext);
                var dataPath = resolveDataPath.GetValueOrThrowIfFailed().DataValue;
                if (!PathHelper.IsJsonPointer(dataPath))
                {
                    dataPath = PathHelper.ToJsonPointer(dataPath);
                }

                var debugContext = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                var dataPathPointer = new PocoJsonPointer(dataPath, this.SchemaReferenceKey, debugContext);
                if (dataPathPointer.IsRelative)
                {
                    var currentActionDataPointer = new PocoJsonPointer(providerContext.CurrentActionDataPath, this.SchemaReferenceKey, debugContext);
                    dataPath = dataPathPointer.ToAbsolute(currentActionDataPointer).ToString();
                }

                await providerContext.AutomationData.ContextManager.LoadEntityAtPath(providerContext, dataPath);
                var data = PocoPathLookupResolver.Resolve(providerContext.AutomationData, dataPath, this.SchemaReferenceKey, debugContext);
                return data;
            }
        }
    }
}
