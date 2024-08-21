// <copyright file="ProductService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Linq;
    using Humanizer;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.ValueTypes;

    public class ProductService : IProductService
    {
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IProductOrganisationSettingRepository productOrganisationSettingRepository;
        private readonly IOrganisationService organisationService;
        private readonly IProductPortalSettingRepository productPortalSettingRepository;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IPortalReadModelRepository portalRepository;

        public ProductService(
            IProductFeatureSettingService productFeatureSettingService,
            IProductOrganisationSettingRepository productOrganisationSettingRepository,
            IOrganisationService organisationService,
            IProductPortalSettingRepository productPortalSettingRepository,
            IPortalReadModelRepository portalRepository,
            IProductConfigurationProvider productConfigurationProvider)
        {
            this.productFeatureSettingService = productFeatureSettingService;
            this.productOrganisationSettingRepository = productOrganisationSettingRepository;
            this.organisationService = organisationService;
            this.productPortalSettingRepository = productPortalSettingRepository;
            this.portalRepository = portalRepository;
            this.productConfigurationProvider = productConfigurationProvider;
        }

        public void ThrowIfProductNotActive(Product product, string productAlias)
        {
            if (product == null)
            {
                throw new ErrorException(Errors.General.NotFound("product", productAlias));
            }

            if (product.Details.Disabled)
            {
                throw new ErrorException(Errors.Product.Disabled(productAlias));
            }

            if (product.Details.Deleted)
            {
                throw new ErrorException(Errors.Product.Deleted(productAlias));
            }
        }

        public async Task ThrowIfProductNotEnabledForOrganisation(Product product, Guid organisationId)
        {
            bool isProductEnabledForOrganisation
                = this.productOrganisationSettingRepository.GetProductOrganisationSettings(
                    product.TenantId,
                    organisationId)
                .Any(p => p.ProductId == product.Id && p.IsNewQuotesAllowed);
            if (!isProductEnabledForOrganisation)
            {
                var organisationReadModelSummary = await this.organisationService
                           .GetOrganisationSummaryForTenantIdAndOrganisationId(product.TenantId, organisationId);
                throw new ErrorException(
                    Errors.Product.NotEnabledForOrganisation(product.Details.Alias, organisationReadModelSummary.Alias));
            }
        }

        public void ThrowIfNewBusinessQuotesDisabled(Product product)
        {
            var productFeature = this.productFeatureSettingService.GetProductFeature(
                product.TenantId,
                product.Id);
            if (!productFeature.AreNewBusinessQuotesEnabled)
            {
                throw new ErrorException(
                    Errors.ProductFeatureSetting.QuoteNewBusinessDisabled(
                        product.Id,
                        product.Details.Name));
            }
        }

        public void ThrowIfNewBusinessPolicyTransactionsDisabled(Product product)
        {
            var productFeature = this.productFeatureSettingService.GetProductFeature(
                product.TenantId,
                product.Id);
            if (!productFeature.AreNewBusinessPolicyTransactionsEnabled)
            {
                throw new ErrorException(
                    Errors.ProductFeatureSetting.PolicyNewBusinessDisabled(
                        product.Id,
                        product.Details.Name));
            }
        }

        public void ThrowIfProductTransactionDisabledForTheLoadOperation(Product product)
        {
            var productFeature = this.productFeatureSettingService.GetProductFeature(
                product.TenantId,
                product.Id);
            if (!productFeature.AreNewBusinessQuotesEnabled)
            {
                throw new ErrorException(
                    Errors.ProductFeatureSetting.QuoteNewBusinessDisabled(
                        product.Id,
                        product.Details.Name));
            }
        }

        public void ThrowIfProductNotEnabledForPortal(Guid tenantId, Product product, Guid portalId)
        {
            var productPortalSettings =
                this.productPortalSettingRepository.GetProductPortalSettings(tenantId, portalId);
            if (!productPortalSettings.Any(p => p.ProductId == product.Id && p.IsNewQuotesAllowed))
            {
                var portal = this.portalRepository.GetPortalById(tenantId, portalId);
                throw new ErrorException(Errors.Product.NotEnabledForPortal(product.Details.Name, portal.Name));
            }
        }

        public async Task ThrowWhenResultingStateIsNotSupported(ReleaseContext releaseContext, Product product, string? initialQuoteState)
        {
            if (initialQuoteState == null || initialQuoteState == StandardQuoteStates.Nascent)
            {
                return;
            }

            var productConfiguration = await this.productConfigurationProvider
                .GetProductConfiguration(releaseContext, WebFormAppType.Quote);
            var workflow = productConfiguration.QuoteWorkflow;
            var isResultingStateSupported = workflow.IsResultingStateSupported(initialQuoteState);
            if (!isResultingStateSupported)
            {
                throw new ErrorException(Errors.Quote.Creation.InitialQuoteStateInvalid(
                    product.Details.Name, initialQuoteState.Camelize()));
            }
        }
    }
}
