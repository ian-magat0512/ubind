// <copyright file="PaymentConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Application.Releases;
    using UBind.Domain.Product;

    /// <summary>
    /// For providing payment configuration.
    /// </summary>
    public class PaymentConfigurationProvider : IPaymentConfigurationProvider
    {
        private readonly IReleaseQueryService releaseQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentConfigurationProvider"/> class.
        /// </summary>
        /// <param name="releaseService">Service for creating releases.</param>
        public PaymentConfigurationProvider(IReleaseQueryService releaseService) =>
            this.releaseQueryService = releaseService;

        /// <summary>
        /// Gets the payment configuration for a given application.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <returns>The payment configuration.</returns>
        public async Task<Maybe<IPaymentConfiguration>> GetConfigurationAsync(ReleaseContext productContext)
        {
            var release = this.releaseQueryService.GetRelease(productContext);
            return await Task.FromResult(release.PaymentConfigurationModel);
        }
    }
}
