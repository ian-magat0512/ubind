// <copyright file="StaticProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System.Threading.Tasks;
    using MorseCode.ITask;

    /// <summary>
    /// Provider for resolving generic-type values.
    /// </summary>
    /// <typeparam name="TValue">The type of value to be provided.</typeparam>
    public class StaticProvider<TValue> : IProvider<TValue>
    {
        private readonly TValue value;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticProvider{TValue}"/> class.
        /// </summary>
        /// <param name="value">The type of value ot be provided.</param>
        public StaticProvider(TValue value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the schema key used for static providers.
        /// </summary>
        /// <remarks>Static providers are used for inline-set values.</remarks>
        public string SchemaReferenceKey => "#value";

        /// <summary>
        /// Resolves the given fixed value.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>A value.</returns>
        public ITask<IProviderResult<TValue>> Resolve(IProviderContext providerContext)
        {
            return Task.FromResult(ProviderResult<TValue>.Success(this.value)).AsITask();
        }
    }
}
