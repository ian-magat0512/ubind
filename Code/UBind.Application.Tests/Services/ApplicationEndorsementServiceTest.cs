// <copyright file="ApplicationEndorsementServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <summary>
    /// Defines the <see cref="ApplicationEndorsementServiceTest" />.
    /// </summary>
    public class ApplicationEndorsementServiceTest
    {
        private static readonly Guid TenantId = TenantFactory.DefaultId;
        private static readonly Guid ProductId = ProductFactory.DefaultId;
        private readonly string quoteNumber = "QuoteNumber";
        private readonly IClock clock = SystemClock.Instance;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider = new DefaultQuoteWorkflowProvider();
        private Guid userId = Guid.NewGuid();
        private Tenant tenant = TenantFactory.Create(TenantId);
        private Mock<ITenantService> tenantService = new Mock<ITenantService>();
        private Mock<IProductFeatureSettingRepository> productFeatureRepository = new Mock<IProductFeatureSettingRepository>();
        private Mock<IProductRepository> productRepository = new Mock<IProductRepository>();

        private Mock<IUserAggregateRepository> userAggregateRepository = new Mock<IUserAggregateRepository>();
        private Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private Mock<IRoleService> roleService = new Mock<IRoleService>();
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<IPolicyService> policyServiceMock = new Mock<IPolicyService>();

        [Fact]
        public async Task ApproveEndorsementQuoteAsync_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.cachingResolver.Setup(c => c.GetProductSettingOrThrow(
                It.IsAny<Guid>(),
                It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);
            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCustomerDetails(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationEndorsement.ApproveEndorsedQuote(releaseContext, quote, null);

            // Assert
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.EndorsementApproval).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task AutoApproveQuoteAsync_WithDisabledProductFeatureSettings_ShoutNotThrowErrorException()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);
            productFeature.Enable(ProductFeatureSettingItem.RenewalQuotes);
            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);
            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.AutoApproveQuote(releaseContext, quote, null);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ApproveReviewedQuoteAsync_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.cachingResolver
                .Setup(c => c.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationEndorsement.ApproveReviewedQuote(releaseContext, quote, formData);

            // Assert
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.ReviewApproval).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task ApproveReviewedQuoteAsync_WithDisabledProductFeatureSettings_ShouldNotThrowErrorException()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ApproveReviewedQuote(releaseContext, quote, formData);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ReferEndorsementQuoteAsync_WithDisabledProductFeatureSettings_ShouldNotThrowErrorException()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCustomerDetails(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ReferQuoteForEndorsement(releaseContext, quote, formData);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ReviewReferralQuoteAsync_WithDisabledProductFeatureSettings_ShouldNotThrowErrorExceptionAsync()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCustomerDetails(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ReferQuoteForReview(
                releaseContext,
                quote,
                formData);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ReturnQuoteAsync_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.cachingResolver
                .Setup(c => c.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);
            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteStateChangedEvent = new QuoteStateChangedEvent(
                TenantId,
                quote.Aggregate.Id,
                quote.Id,
                QuoteAction.ReviewReferral,
                this.userId,
                "Nascent",
                "Review",
                this.clock.Now());
            quote.Apply(quoteStateChangedEvent, 2);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationEndorsement.ReturnQuote(releaseContext, quote, null);

            // Assert
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.Return).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task ReturnQuoteAsync_WithWrongQuoteState_ThrowErrorException()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);
            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteStateChangedEvent = new QuoteStateChangedEvent(
                TenantId,
                quote.Aggregate.Id,
                quote.Id,
                QuoteAction.ReviewReferral,
                this.userId,
                "Nascent",
                "Nascent",
                this.clock.Now());
            quote.Apply(quoteStateChangedEvent, 2);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ReturnQuote(releaseContext, quote, null);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task ReturnQuoteAsync_WithDisabledProductFeatureSettings_ShouldThrowErrorExceptionAsync()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteStateChangedEvent = new QuoteStateChangedEvent(
                TenantId,
                quote.Aggregate.Id,
                quote.Id,
                QuoteAction.ReviewReferral,
                this.userId,
                "Nascent",
                "Review",
                this.clock.Now());
            quote.Apply(quoteStateChangedEvent, 2);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ReturnQuote(releaseContext, quote, null);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task DeclineQuoteAsync_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.cachingResolver
                .Setup(c => c.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationEndorsement.DeclineQuote(releaseContext, quote, null);

            // Assert
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.Decline).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task DeclineQuoteAsync_WithDisabledProductFeatureSettings_ShoudNotThrowErrorExceptionAsync()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
               It.IsAny<Guid>(),
               It.IsAny<Guid>()))
               .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCustomerDetails(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.DeclineQuote(releaseContext, quote, null);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.Decline).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task AutoApproveQuoteAsync_WithEnabledProductFeatureSettings_Succeed()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());
            productFeature.Enable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.cachingResolver
                .Setup(c => c.GetProductSettingOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCalculationResult(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var formData = new Domain.Aggregates.Quote.FormData("{}");
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            await applicationEndorsement.AutoApproveQuote(releaseContext, quote, formData);

            // Assert
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.AutoApproval).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        [Fact]
        public async Task ApproveEndorsementQuoteAsync_WithDisabledProductFeatureSettings_ShouldNotThrowErrorExceptionAsync()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(TenantId, ProductId, this.clock.Now());

            productFeature.Disable(ProductFeatureSettingItem.NewBusinessQuotes);

            var productFeatureSettingService = this.GetProductFeatureService();
            this.productFeatureRepository.Setup(e => e.GetProductFeatureSetting(
                It.IsAny<Guid>(),
                It.IsAny<Guid>()))
                .Returns(productFeature);
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();

            var userAuthenticationData = new UserAuthenticationData(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantId);
            var quoteAggregate = QuoteFactory.WithCustomerDetails(QuoteFactory.WithCustomer(quote.Aggregate), quote.Id);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(this.tenant.Id, quote.Id)).Returns(quoteAggregate);
            var applicationEndorsement = this.GetApplicationEndorsementService(productFeatureSettingService);
            var releaseContext = new ReleaseContext(
                this.tenant.Id,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId ?? Guid.NewGuid());

            // Act
            Func<Task> act = async () => await applicationEndorsement.ApproveEndorsedQuote(releaseContext, quote, null);

            // Assert
            await act.Should().NotThrowAsync<ErrorException>();
            var defaultQuoteWorkflow = new DefaultQuoteWorkflow();
            var expectedResultingState = defaultQuoteWorkflow.GetOperation(QuoteAction.EndorsementApproval).ResultingState;
            quote.LatestQuoteStateChange.ResultingState.Should().Be(expectedResultingState);
        }

        private ProductFeatureSettingService GetProductFeatureService()
        {
            var tenant = TenantFactory.Create(TenantId);
            var product = ProductFactory.Create(ProductId);
            this.tenantService.Setup(t => t.GetTenant(TenantId)).Returns(tenant);
            Mock<DevRelease> devRelease = new Mock<DevRelease>();
            this.productRepository.Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            var productSummary = new Mock<IProductSummary>();
            var deploymentSetting = new ProductDeploymentSetting();
            deploymentSetting.Development = new List<string> { DeploymentEnvironment.Development.ToString() };
            var productDetails = new ProductDetails("name", "productAlias", false, false, this.clock.Now(), deploymentSetting);
            productSummary.Setup(p => p.Details).Returns(productDetails);
            productSummary.Setup(p => p.Id).Returns(ProductId);
            IEnumerable<IProductSummary> productSummaries = new List<IProductSummary>() { productSummary.Object };

            this.productRepository.Setup(p => p.GetAllActiveProductSummariesForTenant(tenant.Id)).Returns(productSummaries);

            var productFeatureService = new ProductFeatureSettingService(
                this.productFeatureRepository.Object,
                this.clock,
                this.cachingResolver.Object);
            return productFeatureService;
        }

        private QuoteEndorsementService GetApplicationEndorsementService(
            ProductFeatureSettingService productFeatureSettingService)
        {
            this.policyServiceMock.Setup(p => p.GenerateQuoteNumber(It.IsAny<ReleaseContext>()))
                .Returns(Task.FromResult(this.quoteNumber));
            var quoteEndorsementService = new QuoteEndorsementService(
                this.roleService.Object,
                this.userAggregateRepository.Object,
                this.httpContextPropertiesResolver.Object,
                this.clock,
                productFeatureSettingService,
                this.quoteWorkflowProvider,
                this.cachingResolver.Object,
                this.policyServiceMock.Object);
            return quoteEndorsementService;
        }
    }
}
