// <copyright file="ComparisonCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Condition based on a function of two data items.
    /// </summary>
    /// <typeparam name="TData">The type of data being compared.</typeparam>
    public class ComparisonCondition<TData> : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<TData>> firstParameterProvider;
        private readonly IProvider<Data<TData>> secondParameterProvider;
        private readonly Func<TData, TData, bool> function;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonCondition{TData}"/> class.
        /// </summary>
        /// <param name="firstParameterProvider">Provider for obtaining the first item to compare.</param>
        /// <param name="secondParameterProvider">Proider for obtaining the second item to compare.</param>
        /// <param name="function">The function for comparing the items.</param>
        public ComparisonCondition(
            IProvider<Data<TData>> firstParameterProvider,
            IProvider<Data<TData>> secondParameterProvider,
            Func<TData, TData, bool> function,
            string providerReferenceKey)
        {
            this.firstParameterProvider = firstParameterProvider;
            this.secondParameterProvider = secondParameterProvider;
            this.function = function;
            this.SchemaReferenceKey = providerReferenceKey;
        }

        public string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            var first = (await this.firstParameterProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var second = (await this.secondParameterProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<bool>>.Success(this.function.Invoke(first, second));
        }
    }
}
