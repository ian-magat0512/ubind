﻿// <copyright file="TextStartsWithFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Expression;

    /// <summary>
    /// For providing a filters that test whether a text expressions begins with text from a second expression.
    /// </summary>
    public class TextStartsWithFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        /// <summary>
        /// Gets the model for building the provider for the text to test.
        /// </summary>
        [JsonProperty]
        public IBuilder<IExpressionProvider> Text { get; private set; }

        /// <summary>
        /// Gets the model for building the provider for the prefix to test for.
        /// </summary>
        [JsonProperty]
        public IBuilder<IExpressionProvider> StartsWith { get; private set; }

        /// <inheritdoc/>
        IFilterProvider IBuilder<IFilterProvider>.Build(IServiceProvider dependencyProvider)
        {
            var startsWithMethod = typeof(string).GetMethod(
                nameof(string.StartsWith), new Type[] { typeof(string) });
            return new BinaryExpressionFilterProvider(
                (e1, e2) => Expression.Call(e1, startsWithMethod, e2),
                this.Text.Build(dependencyProvider),
                this.StartsWith.Build(dependencyProvider),
                "textStartsWithCondition");
        }
    }
}
