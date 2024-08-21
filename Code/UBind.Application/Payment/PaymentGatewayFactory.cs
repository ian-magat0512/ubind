// <copyright file="PaymentGatewayFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Payment.Eway;
    using UBind.Application.Payment.Stripe;
    using UBind.Application.Payment.Zai;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Factory for creating payment gateways.
    /// </summary>
    public class PaymentGatewayFactory
    {
        private readonly IClock clock;
        private readonly IDeftCustomerReferenceNumberGenerator deftCrnGenerator;
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextResolver;
        private readonly ICqrsMediator cqrsMediator;
        private readonly ILogger<PaymentGatewayFactory> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentGatewayFactory"/> class.
        /// </summary>
        /// <param name="clock">A clock.</param>
        /// <param name="deftCrnGenerator">Customer reference number generator.</param>
        /// <param name="logger">A logger.</param>
        public PaymentGatewayFactory(
            IClock clock,
            IDeftCustomerReferenceNumberGenerator deftCrnGenerator,
            ICachingResolver cachingResolver,
            IHttpContextPropertiesResolver httpContextResolver,
            ICqrsMediator mediator,
            ILogger<PaymentGatewayFactory> logger)
        {
            this.clock = clock;
            this.cachingResolver = cachingResolver;
            this.deftCrnGenerator = deftCrnGenerator;
            this.cqrsMediator = mediator;
            this.httpContextResolver = httpContextResolver;
            this.logger = logger;
        }

        /// <summary>
        /// Create a type of payment gateway by name.
        /// </summary>
        /// <param name="configuration">the name of the payment gateway to create.</param>
        /// <returns>A new instance of a payment gateway.</returns>
        public IPaymentGateway Create(IPaymentConfiguration configuration)
        {
            return configuration.Create(this);
        }

        /// <summary>
        /// Create a new instance of an Eway payment gateway.
        /// </summary>
        /// <param name="configuration">The account configuration.</param>
        /// <returns>A new instance of an Eway payment gateway.</returns>
        public IPaymentGateway CreateEwayPaymentGateway(EwayConfiguration configuration)
        {
            return new EwayPaymentGateway(configuration, this.clock, this.logger);
        }

        /// <summary>
        /// Create a new instance of a DEFT payment gateway.
        /// </summary>
        /// <param name="deftConfiguration">The account configuration.</param>
        /// <returns>A new instance of the DEFT payment gateway.</returns>
        internal IPaymentGateway CreateDeftPaymentGateway(DeftConfiguration deftConfiguration)
        {
            var tokenProvider = new DeftAccessTokenProvider(deftConfiguration, this.clock);
            return new DeftPaymentGateway(deftConfiguration, tokenProvider, this.deftCrnGenerator, this.clock);
        }

        /// <summary>
        /// Create a new instance of a Stripe payment gateway.
        /// </summary>
        /// <param name="stripeConfiguration">The account configuration.</param>
        /// <returns>A new instance of the Stripe payment gateway.</returns>
        internal IPaymentGateway CreateStripePaymentGateway(StripeConfiguration stripeConfiguration)
        {
            return new StripePaymentGateway(stripeConfiguration);
        }

        /// <summary>
        /// Create a new instance of a Zai payment gateway.
        /// </summary>
        /// <param name="zaiConfiguration">The account configuration.</param>
        /// <returns>A new instance of the Zai payment gateway.</returns>
        internal IPaymentGateway CreateZaiPaymentGateway(ZaiConfiguration zaiConfiguration)
        {
            var tokenProvider = new ZaiAccessTokenProvider(zaiConfiguration, this.clock);
            return new ZaiPaymentGateway(zaiConfiguration, tokenProvider, this.cachingResolver, this.httpContextResolver, this.cqrsMediator);
        }
    }
}
