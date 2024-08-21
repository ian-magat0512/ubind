// <copyright file="FixedObjectPathLookupConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup
{
    using System;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building an instance of <see cref="FixedObjectPathLookup"/>.
    /// </summary>
    public class FixedObjectPathLookupConfigModel : IBuilder<IObjectPathLookupProvider>
    {
        /// <summary>
        /// Gets or sets the path to use for lookup.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Path { get; set; }

        public string SchemaReferenceKey { get; set; }

        /// <inheritdoc/>
        public IObjectPathLookupProvider Build(IServiceProvider dependencyProvider)
        {
            return new FixedObjectPathLookup(this.Path.Build(dependencyProvider), this.SchemaReferenceKey);
        }
    }
}
