// <copyright file="ActiveDeployedRelease.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Export;
    using UBind.Application.Funding;
    using UBind.Application.Payment;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// For holding release data in the cache for a release that is currently deployed to a give environment.
    /// </summary>
    /// <remarks>
    /// The data exposed by this class is specific to a deployment of a release in a particular environment.
    /// I.e. Configuration that is environment-specific is pre-resolved for the correct environment.
    /// This means we are caching more data in memory than strictly necessary when the same release is deployed
    /// to multiple environments, so in the future we may want to refactor to prevent that.
    /// </remarks>
    public class ActiveDeployedRelease
    {
        private readonly string automationsConfigurationJson;
        private readonly string integrationsConfigurationJson;
        private readonly string paymentConfigurationJson;
        private readonly string fundingConfigurationJson;
        private readonly ProductComponentConfiguration quoteConfiguration;
        private readonly ProductComponentConfiguration claimConfiguration;
        private AutomationsConfigurationModel automationsConfigurationModel;
        private IntegrationConfigurationModel integrationsConfigurationModel;
        private Maybe<IPaymentConfiguration>? paymentConfigurationModel;
        private Maybe<IFundingConfiguration>? fundingConfigurationModel;
        private Instant? lastModifiedTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDeployedRelease"/> class.
        /// </summary>
        /// <param name="release">The release to be cached.</param>
        /// <param name="environment">The environment the cached release is for, for use in parsing environment-specific configuration.</param>
        /// <param name="fieldSerializationBinder">The json serialization binder for field types.</param>
        public ActiveDeployedRelease(
            ReleaseBase release,
            DeploymentEnvironment environment,
            IFieldSerializationBinder fieldSerializationBinder)
        {
            this.ReleaseId = release.Id;
            this.ReleaseContext = new ReleaseContext(
                release.TenantId,
                release.ProductId,
                environment,
                release.Id);
            if (release.QuoteDetails != null)
            {
                this.quoteConfiguration = new ProductComponentConfiguration(release.QuoteDetails, fieldSerializationBinder);
                this.automationsConfigurationJson = release.QuoteDetails.AutomationsJson;
                this.integrationsConfigurationJson = release.QuoteDetails.ExportsJson;
                this.paymentConfigurationJson = release.QuoteDetails.PaymentJson;
                this.fundingConfigurationJson = release.QuoteDetails.FundingJson;
            }

            if (release.ClaimDetails != null)
            {
                this.claimConfiguration = new ProductComponentConfiguration(release.ClaimDetails, fieldSerializationBinder);
            }

            this.lastModifiedTimestamp = release.LastModifiedTimestamp;
        }

        /// <summary>
        /// Gets the ID of the release.
        /// </summary>
        public Guid ReleaseId { get; }

        /// <summary>
        /// Gets the product context the release is cached for.
        /// </summary>
        public ReleaseContext ReleaseContext { get; }

        /// <summary>
        /// Gets the timestamp representing the last synchronization point for the cached release.
        /// This timestamp indicates the latest moment when either quote or claim details were synchronized.
        /// If both quote and claim details exist, it reflects the timestamp of the more recent synchronization.
        /// </summary>
        public Instant? LastModifiedTimestamp
        {
            get { return this.lastModifiedTimestamp; }
        }

        /// <summary>
        /// Gets the automations configuration model for the release.
        /// </summary>
        public AutomationsConfigurationModel AutomationsConfigurationModel
        {
            get
            {
                if (this.automationsConfigurationModel == null)
                {
                    this.automationsConfigurationModel = this.automationsConfigurationJson != null
                        ? AutomationConfigurationParser.Parse(this.automationsConfigurationJson)
                        : new AutomationsConfigurationModel();
                }

                return this.automationsConfigurationModel;
            }
        }

        /// <summary>
        /// Gets the integrations configuration model for the release.
        /// </summary>
        public IntegrationConfigurationModel IntegrationsConfigurationModel
        {
            get
            {
                if (this.integrationsConfigurationModel == null)
                {
                    this.integrationsConfigurationModel = this.integrationsConfigurationJson != null
                        ? IntegrationConfigurationParser.Parse(this.integrationsConfigurationJson)
                        : new IntegrationConfigurationModel();
                }

                return this.integrationsConfigurationModel;
            }
        }

        /// <summary>
        /// Gets the payment configuration model for the release.
        /// </summary>
        public Maybe<IPaymentConfiguration> PaymentConfigurationModel
        {
            get
            {
                if (!this.paymentConfigurationModel.HasValue)
                {
                    if (this.paymentConfigurationJson != null)
                    {
                        var factory = PaymentConfigurationParser.Parse(this.paymentConfigurationJson);
                        var config = factory.Generate(this.ReleaseContext.Environment);
                        this.paymentConfigurationModel = Maybe<IPaymentConfiguration>.From(config);
                    }
                    else
                    {
                        this.paymentConfigurationModel = Maybe<IPaymentConfiguration>.None;
                    }
                }

                return this.paymentConfigurationModel.Value;
            }
        }

        /// <summary>
        /// Gets the funding configuration model for the release.
        /// </summary>
        public Maybe<IFundingConfiguration> FundingConfigurationModel
        {
            get
            {
                if (!this.fundingConfigurationModel.HasValue)
                {
                    if (this.fundingConfigurationJson != null)
                    {
                        var factory = FundingConfigurationParser.Parse(this.fundingConfigurationJson);
                        if (factory == null)
                        {
                            throw new ErrorException(Errors.Payment.Funding.InvalidConfiguration(this.ReleaseContext.Environment, this.ReleaseContext.ProductId));
                        }
                        var config = factory.Generate(this.ReleaseContext.Environment);
                        this.fundingConfigurationModel = Maybe<IFundingConfiguration>.From(config);
                    }
                    else
                    {
                        this.fundingConfigurationModel = Maybe<IFundingConfiguration>.None;
                    }
                }

                return this.fundingConfigurationModel.Value;
            }
        }

        /// <summary>
        /// Gets the app configuration for a given app type.
        /// </summary>
        /// <param name="appType">The app type.</param>
        /// <returns>The app-specific configuration, or null if it doesn't exist.</returns>
        public ProductComponentConfiguration this[WebFormAppType appType]
        {
            get
            {
                if (appType == WebFormAppType.Quote)
                {
                    return this.quoteConfiguration;
                }

                if (appType == WebFormAppType.Claim)
                {
                    return this.claimConfiguration;
                }

                throw new NotSupportedException($"Unsupported app type {appType}");
            }
        }

        /// <summary>
        /// Gets the product component configuration if it exists, otherwise it throws an error.
        /// </summary>
        /// <param name="appType">The app type.</param>
        /// <returns>The app-specific configuration, or throws an error if it doesn't exist.</returns>
        public ProductComponentConfiguration GetProductComponentConfigurationOrThrow(WebFormAppType appType)
        {
            ProductComponentConfiguration componentConfig = this[appType];
            if (componentConfig == null)
            {
                throw new ErrorException(Errors.Product.Component.NotFound(this.ReleaseContext, appType));
            }

            return componentConfig;
        }
    }
}
