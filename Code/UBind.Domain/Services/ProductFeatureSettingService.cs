// <copyright file="ProductFeatureSettingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class ProductFeatureSettingService : IProductFeatureSettingService
    {
        private readonly IProductFeatureSettingRepository productFeatureRepository;
        private readonly IClock clock;
        private readonly ICachingResolver cachingResolver;
        private readonly IDictionary<ProductFeatureSettingItem, Func<Guid, string, Error>> errorMessageMap;
        private readonly Dictionary<ProductFeatureSettingItem, Func<ProductFeatureSetting, bool>> productFeatureGetters;

        public ProductFeatureSettingService(
            IProductFeatureSettingRepository productFeatureRepository,
            IClock clock,
            ICachingResolver cachingResolver)
        {
            this.productFeatureRepository = productFeatureRepository;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
            this.errorMessageMap = new Dictionary<ProductFeatureSettingItem, Func<Guid, string, Error>>
            {
                {
                    ProductFeatureSettingItem.NewBusinessQuotes,
                    Errors.ProductFeatureSetting.QuoteNewBusinessDisabled
                },
                {
                    ProductFeatureSettingItem.AdjustmentQuotes,
                    (id, name) => this.GetQuoteTransactionTypeDisabledMessage(id, name, "Adjustment")
                },
                {
                    ProductFeatureSettingItem.RenewalQuotes,
                    (id, name) => this.GetQuoteTransactionTypeDisabledMessage(id, name, "Renewal")
                },
                {
                    ProductFeatureSettingItem.CancellationQuotes,
                    (id, name) => this.GetQuoteTransactionTypeDisabledMessage(id, name, "Cancellation")
                },
                {
                    ProductFeatureSettingItem.NewBusinessPolicyTransactions,
                    Errors.ProductFeatureSetting.PolicyNewBusinessDisabled
                },
                {
                    ProductFeatureSettingItem.AdjustmentPolicyTransactions,
                    (id, name) => this.GetPolicyTransactionTypeDisabledMessage(id, name, "Adjustment")
                },
                {
                    ProductFeatureSettingItem.RenewalPolicyTransactions,
                    (id, name) => this.GetPolicyTransactionTypeDisabledMessage(id, name, "Renewal")
                },
                {
                    ProductFeatureSettingItem.CancellationPolicyTransactions,
                    (id, name) => this.GetPolicyTransactionTypeDisabledMessage(id, name, "Cancellation")
                },
            };
            this.productFeatureGetters = new Dictionary<ProductFeatureSettingItem, Func<ProductFeatureSetting, bool>>
            {
                { ProductFeatureSettingItem.NewBusinessQuotes, productFeatureSetting => productFeatureSetting.AreNewBusinessQuotesEnabled },
                { ProductFeatureSettingItem.AdjustmentQuotes, productFeatureSetting => productFeatureSetting.AreAdjustmentQuotesEnabled },
                { ProductFeatureSettingItem.RenewalQuotes, productFeatureSetting => productFeatureSetting.AreRenewalQuotesEnabled },
                { ProductFeatureSettingItem.CancellationQuotes, productFeatureSetting => productFeatureSetting.AreCancellationQuotesEnabled },
                { ProductFeatureSettingItem.NewBusinessPolicyTransactions, productFeatureSetting => productFeatureSetting.AreNewBusinessPolicyTransactionsEnabled },
                { ProductFeatureSettingItem.AdjustmentPolicyTransactions, productFeatureSetting => productFeatureSetting.AreAdjustmentPolicyTransactionsEnabled },
                { ProductFeatureSettingItem.RenewalPolicyTransactions, productFeatureSetting => productFeatureSetting.AreRenewalPolicyTransactionsEnabled },
                { ProductFeatureSettingItem.CancellationPolicyTransactions, productFeatureSetting => productFeatureSetting.AreCancellationPolicyTransactionsEnabled },
            };
        }

        /// <summary>
        /// Get product feature type by quote type.
        /// </summary>
        /// <param name="quoteType">The Quote Type.</param>
        /// <returns>the product feature type.</returns>
        public static ProductFeatureSettingItem GetQuoteProductFeatureByQuoteType(QuoteType quoteType)
        {
            switch (quoteType)
            {
                case QuoteType.NewBusiness:
                    return ProductFeatureSettingItem.NewBusinessQuotes;
                case QuoteType.Renewal:
                    return ProductFeatureSettingItem.RenewalQuotes;
                case QuoteType.Adjustment:
                    return ProductFeatureSettingItem.AdjustmentQuotes;
                case QuoteType.Cancellation:
                    return ProductFeatureSettingItem.CancellationQuotes;
                default:
                    throw new ErrorException(
                        Errors.General.ModelValidationFailed($"{quoteType} is not a valid quote type."));
            }
        }

        /// <summary>
        /// Get product feature type by quote type.
        /// </summary>
        /// <param name="quoteType">The Quote Type.</param>
        /// <returns>the product feature type.</returns>
        public static ProductFeatureSettingItem GetPolicyProductFeatureByQuoteType(QuoteType quoteType)
        {
            switch (quoteType)
            {
                case QuoteType.NewBusiness:
                    return ProductFeatureSettingItem.NewBusinessPolicyTransactions;
                case QuoteType.Renewal:
                    return ProductFeatureSettingItem.RenewalPolicyTransactions;
                case QuoteType.Adjustment:
                    return ProductFeatureSettingItem.AdjustmentPolicyTransactions;
                case QuoteType.Cancellation:
                    return ProductFeatureSettingItem.CancellationPolicyTransactions;
                default:
                    throw new ErrorException(
                        Errors.General.ModelValidationFailed($"{quoteType} is not a valid quote type."));
            }
        }

        /// <inheritdoc/>
        public List<ProductFeatureSetting> GetEnabledDeployedProductFeatureSettings(Guid tenantId, DeploymentEnvironment environment)
        {
            return this.cachingResolver.GetDeployedProductSettingsOrThrow(tenantId, environment);
        }

        /// <inheritdoc/>
        public ProductFeatureSetting EnableProductFeature(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureSettingItem)
        {
            var productFeature = this.GetProductFeature(tenantId, productId);
            if (productFeature.IsProductFeatureEnabled(productFeatureSettingItem))
            {
                throw new ErrorException(Errors.ProductFeatureSetting.AlreadyEnabled(
                    tenantId, productId, productFeatureSettingItem.Humanize()));
            }

            this.cachingResolver.RemoveCachedProductSettings(tenantId, productId);
            return this.productFeatureRepository.EnableProductFeature(tenantId, productId, productFeatureSettingItem);
        }

        /// <inheritdoc/>
        public ProductFeatureSetting DisableProductFeature(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureSettingItem)
        {
            var productFeature = this.GetProductFeature(tenantId, productId);
            if (!productFeature.IsProductFeatureEnabled(productFeatureSettingItem))
            {
                throw new ErrorException(Errors.ProductFeatureSetting.AlreadyDisabled(
                    tenantId, productId, productFeatureSettingItem.Humanize()));
            }

            this.cachingResolver.RemoveCachedProductSettings(tenantId, productId);
            return this.productFeatureRepository.DisableProductFeature(tenantId, productId, productFeatureSettingItem);
        }

        /// <inheritdoc/>
        public async Task ThrowIfFeatureIsNotEnabled(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType,
            string operationName)
        {
            var isProductFeatureSettingEnabled = this.IsProductFeatureSettingEnabled(tenantId, productId, productFeatureType);
            if (!isProductFeatureSettingEnabled)
            {
                var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
                var errorMessage = this.GetErrorMessage(productFeatureType, productId, product.Details.Name);

                throw new ErrorException(errorMessage);
            }
        }

        /// <inheritdoc/>
        public bool IsProductFeatureSettingEnabled(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType)
        {
            var productFeatureSettings = this.cachingResolver.GetProductSettingOrThrow(tenantId, productId);
            if (productFeatureSettings == null)
            {
                throw new ErrorException(Errors.ProductFeatureSetting.NotFound(tenantId, productId));
            }

            return this.GetFeatureEnabledProperty(productFeatureType, productFeatureSettings);
        }

        /// <inheritdoc/>
        public void CreateProductFeatures(
            Guid tenantId,
            Guid productId)
        {
            var productFeature =
                new ProductFeatureSetting(
                    tenantId,
                    productId,
                    this.clock.Now());
            this.productFeatureRepository.AddProductFeatureSetting(productFeature);
        }

        /// <inheritdoc/>
        public ProductFeatureSetting GetProductFeature(
            Guid tenantId,
            Guid productId)
        {
            return this.cachingResolver.GetProductSettingOrThrow(tenantId, productId);
        }

        private bool GetFeatureEnabledProperty(ProductFeatureSettingItem productFeatureType, ProductFeatureSetting productFeature)
        {
            if (this.productFeatureGetters.TryGetValue(productFeatureType, out var propertyGetter))
            {
                return propertyGetter(productFeature);
            }
            else
            {
                throw new ErrorException(
                    Errors.General.ModelValidationFailed($"{productFeatureType} is not a valid Product feature."));
            }
        }

        private Error GetErrorMessage(ProductFeatureSettingItem featureType, Guid productId, string productName)
        {
            if (this.errorMessageMap.TryGetValue(featureType, out var errorMessageDelegate))
            {
                return errorMessageDelegate(productId, productName);
            }

            return Errors.General.ModelValidationFailed($"{featureType} is not a valid Product feature.");
        }

        private Error GetQuoteTransactionTypeDisabledMessage(Guid productId, string productName, string transactionType)
        {
            return Errors.ProductFeatureSetting.QuoteTypeDisabled(productId, productName, transactionType);
        }

        private Error GetPolicyTransactionTypeDisabledMessage(Guid productId, string productName, string transactionType)
        {
            return Errors.ProductFeatureSetting.PolicyTransactionTypeDisabled(productId, productName, transactionType);
        }
    }
}
