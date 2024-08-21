// <copyright file="ConstantExpressionProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for building constant expressions for use in filters from literals.
    /// </summary>
    public class ConstantExpressionProviderConfigModel : IBuilder<IExpressionProvider>
    {
        private readonly IBuilder<IProvider<IData>> providerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionProviderConfigModel"/> class.
        /// </summary>
        /// <param name="providerBuilder">A builder for a generic provider.</param>
        public ConstantExpressionProviderConfigModel(IBuilder<IProvider<IData>> providerBuilder) =>
            this.providerBuilder = providerBuilder;

        /// <inheritdoc/>
        public IExpressionProvider Build(IServiceProvider serviceProvider)
        {
            return new ConstantExpressionProvider(this.providerBuilder.Build(serviceProvider));
        }
    }
}
