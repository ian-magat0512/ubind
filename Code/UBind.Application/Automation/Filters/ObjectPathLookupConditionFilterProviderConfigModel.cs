// <copyright file="ObjectPathLookupConditionFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building object path lookup conditions to be used for filtering lists.
    /// </summary>
    /// <remarks>Since this path lookup returns a boolean value, it needs to be separated from
    /// other expression providers than return other data-types.</remarks>
    public class ObjectPathLookupConditionFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        public ObjectPathLookupConditionFilterProviderConfigModel(
            IBuilder<IProvider<Data<string>>> path, IBuilder<IProvider<IData>> valueIfNotFound, IBuilder<IObjectProvider> dataObject)
        {
            this.Path = path;
            this.ValueIfNotFound = valueIfNotFound;
            this.DataObject = dataObject;
        }

        /// <summary>
        /// Gets or sets a provider for the path.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Path { get; }

        /// <summary>
        /// Gets or sets the default value provider to be used when path value cannot be resolved.
        /// </summary>
        public IBuilder<IProvider<IData>> ValueIfNotFound { get; }

        /// <summary>
        /// Gets or sets a provider for the object to look in.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new ObjectPathLookupConditionFilterProvider(
                this.Path.Build(dependencyProvider),
                this.ValueIfNotFound?.Build(dependencyProvider),
                this.DataObject?.Build(dependencyProvider));
        }
    }
}
