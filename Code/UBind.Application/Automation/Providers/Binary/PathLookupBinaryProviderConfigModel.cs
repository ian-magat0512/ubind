﻿// <copyright file="PathLookupBinaryProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Binary
{
    using System;
    using UBind.Application.Automation.PathLookup;

    /// <summary>
    /// Model for building an instance of <see cref="PathLookupBinaryProvider"/>.
    /// </summary>
    public class PathLookupBinaryProviderConfigModel : IBuilder<IProvider<Data<byte[]>>>
    {
        /// <summary>
        /// Gets or sets the object path lookup to be used.
        /// </summary>
        public IBuilder<IObjectPathLookupProvider> PathLookup { get; set; }

        /// <summary>
        /// Gets or sets an optional value that should be returned if no value is found when the path is resolved.
        /// </summary>
        public IBuilder<IProvider<Data<byte[]>>> ValueIfNotFound { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<byte[]>> Build(IServiceProvider dependencyProvider)
        {
            return new PathLookupBinaryProvider(
                this.PathLookup.Build(dependencyProvider),
                this.ValueIfNotFound?.Build(dependencyProvider));
        }
    }
}
