// <copyright file="PaymentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System.Threading.Tasks;
    using UBind.Application.Payment;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <summary>
    /// Responsible for making payments via payment gateways.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentConfigurationProvider paymentConfigurationProvider;
        private readonly PaymentGatewayFactory paymentGatewayFactory;

        public PaymentService(
            IPaymentConfigurationProvider paymentConfigurationProvider,
            PaymentGatewayFactory paymentGatewayFactory)
        {
            this.paymentConfigurationProvider = paymentConfigurationProvider;
            this.paymentGatewayFactory = paymentGatewayFactory;
        }

        /// <inheritdoc/>
        public async Task<bool> CanCalculateMerchantFees(ReleaseContext releaseContext)
        {
            var productPaymentConfigurationResult = await this.paymentConfigurationProvider.GetConfigurationAsync(releaseContext);
            if (productPaymentConfigurationResult.HasNoValue)
            {
                return false;
            }

            var productPaymentConfiguration = productPaymentConfigurationResult.Value;
            var gateway = this.paymentGatewayFactory.Create(productPaymentConfiguration);
            return gateway.CanCalculateMerchantFees();
        }

        /// <inheritdoc/>
        public async Task<MerchantFees> CalculateMerchantFees(
            ReleaseContext releaseContext,
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData)
        {
            var productPaymentConfigurationResult =
                await this.paymentConfigurationProvider.GetConfigurationAsync(releaseContext);
            if (productPaymentConfigurationResult.HasNoValue)
            {
                throw new ErrorException(Errors.Payment.NotConfigured(releaseContext));
            }

            var productPaymentConfiguration = productPaymentConfigurationResult.Value;
            IPaymentGateway gateway = this.paymentGatewayFactory.Create(productPaymentConfiguration);
            return await gateway.CalculateMerchantFees(payableAmount, currencyCode, paymentData);
        }
    }
}
