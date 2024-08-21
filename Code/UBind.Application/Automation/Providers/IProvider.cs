// <copyright file="IProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers
{
    using MorseCode.ITask;

    /// <summary>
    /// Represents something which can resolve a value.
    /// </summary>
    /// <typeparam name="TDataValue">The type of the value to be resolved.</typeparam>
    public interface IProvider<out TDataValue>
    {
        /// <summary>
        /// Gets the reference key for the given provider within the automation schema.
        /// </summary>
        string SchemaReferenceKey { get; }

        /// <summary>
        /// Resolves the value which the provider is expected to provide.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The value which this provider provides.</returns>
        ITask<IProviderResult<TDataValue>> Resolve(IProviderContext providerContext);
    }
}
