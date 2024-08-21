// <copyright file="ObjectPathLookupProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.PathLookup
{
    using System;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for building an instance of <see cref="ObjectPathLookupProvider"/>.
    /// </summary>
    public class ObjectPathLookupProviderConfigModel : IBuilder<IObjectPathLookupProvider>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the path to be used to lookup a data object.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the data object to be queried on. If not set, AutomationData is used.
        /// </summary>
        public IBuilder<IObjectProvider>? DataObject { get; set; }

        public string SchemaReferenceKey { get; set; } = "objectPathLookup";

        /// <inheritdoc/>
        public IObjectPathLookupProvider Build(IServiceProvider dependencyProvider)
        {
            return new ObjectPathLookupProvider(
                this.Path.Build(dependencyProvider),
                this.DataObject?.Build(dependencyProvider),
                this.SchemaReferenceKey);
        }
    }
}
