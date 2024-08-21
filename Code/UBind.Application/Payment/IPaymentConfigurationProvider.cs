// <copyright file="IPaymentConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Product;

    /// <summary>
    /// Interface for obtaining configurations for an quote.
    /// </summary>
    public interface IPaymentConfigurationProvider
    {
        /// <summary>
        /// Gets a given type of configuration for an quote.
        /// </summary>
        /// <param name="releaseContext">The release context.</param>
        /// <returns>A task whose result is the configuration.</returns>
        Task<Maybe<IPaymentConfiguration>> GetConfigurationAsync(ReleaseContext productContext);
    }
}
