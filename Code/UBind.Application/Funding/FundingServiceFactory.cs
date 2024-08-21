// <copyright file="FundingServiceFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Funding.EFundExpress;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Application.Funding.RedPlanetPremiumFunding;
    using UBind.Application.Funding.RedPlanetPremiumFunding.Arteva;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Funding;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <summary>
    /// Factory for creating payment gateways.
    /// </summary>
    public class FundingServiceFactory
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IClock clock;
        private readonly IFundingServiceRedirectUrlHelper urlHelper;
        private readonly ICachingAccessTokenProvider accessTokenProvider;
        private readonly IIqumulateService iqumulateFundingService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingServiceFactory"/> class.
        /// </summary>
        /// <param name="quoteAggregateResolverService">The quote aggregate resolver service.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="urlHelper">Helper for generating URLs for redirection from funding sites.</param>
        /// <param name="accessTokenProvider">Service for obtaining access tokens for Premium Funding Company's API.</param>
        public FundingServiceFactory(
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IClock clock,
            IFundingServiceRedirectUrlHelper urlHelper,
            ICachingAccessTokenProvider accessTokenProvider,
            ICachingResolver cachingResolver,
            IIqumulateService iqumulateFundingService,
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IServiceProvider serviceProvider)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.clock = clock;
            this.urlHelper = urlHelper;
            this.accessTokenProvider = accessTokenProvider;
            this.iqumulateFundingService = iqumulateFundingService;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a type of funding service using a given configuration.
        /// </summary>
        /// <param name="configuration">The configuration for creating the funding service.</param>
        /// <param name="releaseContext">The current release context</param>
        /// <returns>A new instance of a payment gateway.</returns>
        public IFundingService Create(IFundingConfiguration configuration, ReleaseContext releaseContext)
        {
            configuration.SetReleaseContext(releaseContext);
            return configuration.Create(this);
        }

        /// <summary>
        /// Create a new instance of a service for Premium Funding (the company).
        /// </summary>
        /// <param name="configuration">The funding service configuration.</param>
        /// <returns>A new instance of PremiumFundingService using the given configuration.</returns>
        public PremiumFundingService CreatePremiumFundingService(IPremiumFundingConfiguration configuration)
        {
            return new PremiumFundingService(this.quoteAggregateResolverService, configuration, this.accessTokenProvider, this.cachingResolver, this.clock);
        }

        /// <summary>
        /// Create a new instance of a service for Principal Finance.
        /// </summary>
        /// <param name="configuration">The funding service configuration.</param>
        /// <returns>A new instance of PremiumFundingService using the given configuration.</returns>
        public EfundExpressService CreateEfundExpressFundingService(IEFundExpressProductConfiguration configuration)
        {
            return new EfundExpressService(configuration, this.urlHelper, this.cachingResolver, this.clock);
        }

        /// <summary>
        /// Create a new instance of a service for iQumulate.
        /// </summary>
        /// <param name="configuration">The funding service configuration.</param>
        /// <returns>A new instance of PremiumFundingService using the given configuration.</returns>
        public IFundingService CreateIQumulateService(IIqumulateConfiguration configuration)
        {
            this.iqumulateFundingService.ValidateConfiguration(configuration);
            return this.iqumulateFundingService;
        }

        /// <summary>
        /// Create a new instance of a service for Red Planet Odyssey Funding API.
        /// </summary>
        /// <param name="configuration">The funding service configuration.</param>
        /// <returns>A new instance of RedPlanetPremiumFundingService using the given configuration.</returns>
        public IFundingService CreateRedPlanetPremiumFundingService(IRedPlanetFundingConfiguration configuration)
        {
            switch (configuration.FundingType)
            {
                case RedPlanetFundingType.Arteva:
                    var logger = this.serviceProvider.GetRequiredService<ILogger<RedPlanetPremiumFundingApiClient>>();
                    var apiClient = new RedPlanetPremiumFundingApiClient(configuration, this.cachingResolver, logger);
                    return new RedPlanetPremiumFundingService(
                        configuration,
                        this.clock,
                        this.cachingResolver,
                        apiClient,
                        this.quoteAggregateRepository,
                        this.httpContextPropertiesResolver);
                default:
                    throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("Funding Type defined in the configuration is not supported."));
            }
        }
    }
}
