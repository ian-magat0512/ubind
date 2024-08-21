// <copyright file="ObjectPathLookupExpressionProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using NodaTime;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Provider for obtaining an expression for data from an object path.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class ObjectPathLookupExpressionProviderConfigModel<TData> : IDataExpressionProviderConfigModel<TData>, IBuilder<IExpressionProvider>
    {
        /// <summary>
        /// Gets or sets a provider for the path.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Path { get; set; }

        /// <summary>
        /// Gets or sets the default value provider to be used when path value cannot be resolved.
        /// </summary>
        public IBuilder<IProvider<IData>> ValueIfNotFound { get; set; }

        /// <summary>
        /// Gets or sets a provider for the object to look in.
        /// </summary>
        public IBuilder<IObjectProvider> DataObject { get; set; }

        /// <inheritdoc/>
        public IExpressionProvider Build(IServiceProvider dependencyProvider)
        {
            if (typeof(TData) == typeof(LocalTime))
            {
                return new ObjectPathLookupExpressionTimeProvider<TData>(
                    this.Path.Build(dependencyProvider),
                    this.ValueIfNotFound?.Build(dependencyProvider),
                    this.DataObject?.Build(dependencyProvider));
            }

            if (typeof(TData) == typeof(LocalDate))
            {
                return new ObjectPathLookupExpressionDateProvider<TData>(
                    this.Path.Build(dependencyProvider),
                    this.ValueIfNotFound?.Build(dependencyProvider),
                    this.DataObject?.Build(dependencyProvider));
            }

            return new ObjectPathLookupExpressionProvider<TData>(
                this.Path.Build(dependencyProvider),
                this.ValueIfNotFound?.Build(dependencyProvider),
                this.DataObject?.Build(dependencyProvider));
        }
    }
}
