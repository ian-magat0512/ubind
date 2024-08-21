// <copyright file="ContextEntitiesObjectProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StackExchange.Profiling;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="ContextEntitiesObjectProvider"/> from the JSON configuration.
    /// </summary>
    public class ContextEntitiesObjectProviderConfigModel : IBuilder<IObjectProvider>
    {
        /// <summary>
        /// Gets or sets the list of JSON pointers identifying properties (or hierarchies of properties) within the automation context, that should be included in the result object.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>> ContextEntities { get; set; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        /// <inheritdoc/>
        public IObjectProvider Build(IServiceProvider dependencyProvider)
        {
            using (MiniProfiler.Current.Step(nameof(ContextEntitiesObjectProviderConfigModel) + "." + nameof(this.Build)))
            {
                return new ContextEntitiesObjectProvider(
                this.ContextEntities.Select(pointer => pointer.Build(dependencyProvider)));
            }
        }
    }
}
