// <copyright file="ObjectPathLookupProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.PathLookup
{
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Reads a value from a data object using a path defined by a text provider.
    /// </summary>
    public class ObjectPathLookupProvider : IObjectPathLookupProvider
    {
        private readonly IProvider<Data<string>> pathProvider;
        private readonly IObjectProvider? dataObjectProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPathLookupProvider"/> class.
        /// </summary>
        /// <param name="pathProvider">The path provider.</param>
        /// <param name="dataObjectProvider">The data object provider.</param>
        public ObjectPathLookupProvider(
            IProvider<Data<string>> pathProvider, IObjectProvider? dataObjectProvider, string schemaReferenceKey)
        {
            this.pathProvider = pathProvider;
            this.dataObjectProvider = dataObjectProvider;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        /// <summary>
        /// Gets the schema reference key for this provider.
        /// </summary>
        /// <remarks>Reference key to be used should be from the type-specific implementation.</remarks>
        public string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public async ITask<IProviderResult<IData>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(ObjectPathLookupProvider) + "." + nameof(this.Resolve)))
            {
                var resolvePath = await this.pathProvider.Resolve(providerContext);
                string path = resolvePath.GetValueOrThrowIfFailed().DataValue;
                var dataObject = (await this.dataObjectProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                dataObject ??= providerContext.AutomationData;
                return PocoPathLookupResolver.Resolve(
                    dataObject,
                    path,
                    this.SchemaReferenceKey,
                    await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey));
            }
        }
    }
}
