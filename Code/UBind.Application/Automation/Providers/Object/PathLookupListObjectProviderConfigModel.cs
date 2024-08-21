// <copyright file="PathLookupListObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Object;

    /// <summary>
    /// This class is needed to build an instance of <see cref="PathLookupListObjectProvider"/>.
    /// </summary>
    public class PathLookupListObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupListObjectProviderConfigModel"/> class.
        /// </summary>
        /// <param name="properties">The list of the properties that should be created on the output object.</param>
        /// <param name="dataObject">The object to read the value from. If omitted, the automation data will be used by default.</param>
        [JsonConstructor]
        public PathLookupListObjectProviderConfigModel(
            IEnumerable<ObjectPathPropertyConfigModel> properties,
            IBuilder<IObjectProvider> dataObject)
        {
            this.Properties = properties;
            this.DataObject = dataObject;
        }

        /// <summary>
        /// Gets the list of the properties that should be created on the output object.
        /// </summary>
        public IEnumerable<ObjectPathPropertyConfigModel> Properties { get; }

        /// <summary>
        /// Gets an optional object to read the value from. If omitted, the automation data will be used by default.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; }

        /// <inheritdoc/>
        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            var properties = this.Properties.Select(prop => prop.Build(dependencyProvider)).ToList();
            var dataObject = this.DataObject?.Build(dependencyProvider);
            return new PathLookupListObjectProvider(properties, dataObject);
        }
    }
}
