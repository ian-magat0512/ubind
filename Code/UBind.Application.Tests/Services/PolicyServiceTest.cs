// <copyright file="PolicyServiceTest.cs" company="uBind">
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
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Aggregates.Quote;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="PolicyServiceTest"/>.
    /// </summary>
    public class PolicyServiceTest
    {
        private readonly Product product = ProductFactory.Create(Guid.NewGuid());
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private readonly Mock<IPolicyNumberRepository> numberRepository = new Mock<IPolicyNumberRepository>();
        private readonly ISystemAlertService systemAlertService = new Mock<ISystemAlertService>().Object;
        private readonly IProductConfigurationProvider productConfigProvider = new DefaultProductConfigurationProvider();
        private Mock<IProductFeatureSettingService> productFeatureSettingService = new Mock<IProductFeatureSettingService>();
        private Mock<IClaimReadModelRepository> claimReadModelRepository = new Mock<IClaimReadModelRepository>();
        private IPolicyService policyService;
        private TestClock clock = new TestClock();
        private Tenant tenant = TenantFactory.Create(TenantFactory.DefaultId);
        private Quote quote;

        public PolicyServiceTest()
        {
            this.policyService = new PolicyService(
                 this.quoteAggregateRepository.Object,
                 this.clock,
                 new Mock<ICachingResolver>().Object,
                 new Mock<IClaimReadModelRepository>().Object,
                 this.productConfigProvider,
                 new Mock<IUniqueIdentifierService>().Object,
                 new Mock<IQuoteDocumentReadModelRepository>().Object,
                 this.numberRepository.Object,
                 new Mock<IPolicyReadModelRepository>().Object,
                 this.systemAlertService,
                 new Mock<IQuoteReferenceNumberGenerator>().Object,
                 new Mock<IHttpContextPropertiesResolver>().Object,
                 this.productFeatureSettingService.Object,
                new Mock<IQuoteWorkflowProvider>().Object,
                new Mock<IPolicyTransactionTimeOfDayScheme>().Object,
                new Mock<IUBindDbContext>().Object);

            var policy = QuoteFactory.CreateNewPolicy(this.tenant.Id, this.product.Id, DeploymentEnvironment.Development);
            this.quote = policy.GetQuoteOrThrow(policy.Policy.QuoteId.GetValueOrDefault());
            this.quote = this.quote.Aggregate.GetQuoteOrThrow(this.quote.Id);
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsRefundsAreAlwaysProvided()
        {
            // Arrange
            this.SetCancellationSetting(RefundRule.RefundsAreAlwaysProvided);
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnFalse_WhenRefundRuleIsRefundsAreNeverProvided()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreNeverProvided);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeFalse();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsRefundsAreProvidedIfNoClaimsWereMadeDuringCurrentPolicyPeriodAndNoClaimsMade()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.CurrentPolicyPeriod, 0);
            IEnumerable<IClaimReadModelSummary> claimReadModel = Array.Empty<IClaimReadModelSummary>();
            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claimReadModel);
            this.policyService = this.GetPolicyService(this.productFeatureSettingService.Object, this.claimReadModelRepository.Object);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnFalse_WhenRefundRuleIsRefundsAreProvidedIfNoClaimsMadeDuringCurrentPolicyPeriodAndThereAreClaimsMade()
        {
            // Arrange
            var currentDateTime = this.clock.GetCurrentInstant().ToLocalDateTimeInAet();
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.CurrentPolicyPeriod);

            var claimWithCompleteStatus = new Mock<IClaimReadModelSummary>();
            claimWithCompleteStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Complete));
            claimWithCompleteStatus.Setup(e => e.IncidentDateTime).Returns(currentDateTime.PlusDays(1));
            var claimWithWithdrawnStatus = new Mock<IClaimReadModelSummary>();
            claimWithWithdrawnStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Withdrawn));
            claimWithWithdrawnStatus.Setup(e => e.IncidentDateTime).Returns(currentDateTime.PlusDays(1));
            IEnumerable<IClaimReadModelSummary> claims = new IClaimReadModelSummary[]
            {
               claimWithCompleteStatus.Object,
               claimWithWithdrawnStatus.Object,
            };

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claims);
            this.policyService = this.GetPolicyService(this.productFeatureSettingService.Object, this.claimReadModelRepository.Object);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeFalse();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsRefundsAreProvidedIfNoClaimsMadeDuringCurrentPolicyPeriodAndThereAreClaimsMadeInWithDrawnAndDeclineStatus()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.SetCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.CurrentPolicyPeriod);
            var claimWithWithdrawnStatus = new Mock<IClaimReadModelSummary>();
            claimWithWithdrawnStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Withdrawn));
            var claimWithDeclinedStatus = new Mock<IClaimReadModelSummary>();
            claimWithDeclinedStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Declined));
            IEnumerable<IClaimReadModelSummary> claims = new IClaimReadModelSummary[]
            {
               claimWithWithdrawnStatus.Object,
               claimWithDeclinedStatus.Object,
            };

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claims);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsRefundsAreProvidedIfNoClaimsWereMadeDuringLifeTimeOfThePolicyAndNoClaimsMade()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.LifeTimeOfThePolicy);
            IEnumerable<IClaimReadModelSummary> emptyClaimns = Array.Empty<IClaimReadModelSummary>();

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(emptyClaimns);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsRefundsAreProvidedIfNoClaimsWereMadeDuringLifeTimeOfThePolicyAndClaimsMade()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.LifeTimeOfThePolicy);
            var claimReadModelSummary = new Mock<IClaimReadModelSummary>();
            IEnumerable<IClaimReadModelSummary> claimReadModel = Array.Empty<IClaimReadModelSummary>();

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claimReadModel);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsProvidedIfNoClaimsMadeDuringTheLastTwoYearsAndNoClaimsMade()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.LastNumberOfYears, 2);
            var claimReadModelSummary = new Mock<IClaimReadModelSummary>();
            IEnumerable<IClaimReadModelSummary> claimReadModel = Array.Empty<IClaimReadModelSummary>();

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claimReadModel);

            this.policyService = this.GetPolicyService(this.productFeatureSettingService.Object, this.claimReadModelRepository.Object);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnFalse_WhenRefundRuleIsProvidedIfNoClaimsMadeDuringTheLastTwoYearsAndThereAreClaimsMade()
        {
            // Arrange
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsAreProvidedIfNoClaimsWereMade, PolicyPeriodCategory.LastNumberOfYears, 2);
            var claimWithCompleteStatus = new Mock<IClaimReadModelSummary>();
            claimWithCompleteStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Complete));
            var claimWithDeclinedStatus = new Mock<IClaimReadModelSummary>();
            claimWithDeclinedStatus.Setup(e => e.Status).Returns(nameof(ClaimState.Incomplete));
            var currentDate = new LocalDateTime(this.clock.Today().Year, this.clock.Today().Month, this.clock.Today().Day, 0, 0, 0);
            claimWithCompleteStatus.Setup(e => e.IncidentDateTime).Returns(currentDate);
            IEnumerable<IClaimReadModelSummary> claims = new IClaimReadModelSummary[]
            {
               claimWithCompleteStatus.Object,
               claimWithDeclinedStatus.Object,
            };

            this.claimReadModelRepository.Setup(e => e.ListAllClaimsByCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<EntityListFilters>())).Returns(claims);
            this.policyService = this.GetPolicyService(this.productFeatureSettingService.Object, this.claimReadModelRepository.Object);

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                null,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeFalse();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnTrue_WhenRefundRuleIsManuallySelectedDuringReviewAndProvideRefundDataLocatorIsSetToTrue()
        {
            // Arrange
            var provideRefund = true;
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsCanOptionallyBeProvided);
            var dataLocators = new DataLocators();
            dataLocators.IsRefundApproved = new System.Collections.Generic.List<DataLocation>();
            dataLocators.IsRefundApproved.Add(new DataLocation(DataSource.Calculation, "questions.cancellation.provideRefund"));
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, dataLocators);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData(provideRefund));

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                quoteDataRetriever,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeTrue();
        }

        [Fact]
        public void IsRefundAllowed_ShouldReturnFalse_WhenRefundRuleIsManuallySelectedDuringReviewAndProvideRefundDataLocatorIsSetToFalse()
        {
            // Arrange
            var provideRefund = false;
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
            productFeature.UpdateCancellationSetting(RefundRule.RefundsCanOptionallyBeProvided);
            var dataLocators = new DataLocators();
            dataLocators.IsRefundApproved = new System.Collections.Generic.List<DataLocation>();
            dataLocators.IsRefundApproved.Add(new DataLocation(DataSource.Calculation, "questions.cancellation.provideRefund"));
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, dataLocators);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData(provideRefund));

            // Act
            var isRefundAllowed = this.policyService.IsRefundAllowed(
                this.quote.Aggregate.Policy,
                quoteDataRetriever,
                productFeature);

            // Assert
            isRefundAllowed.Should().BeFalse();
        }

        private void SetCancellationSetting(RefundRule cancellationRefundRule, PolicyPeriodCategory? policyDuration = null, int? lastNumberOfYearsWhichNoClaimsMade = null)
        {
            var productFeature = new ProductFeatureSetting(this.tenant.Id, this.product.Id, this.clock.Now());
            productFeature.UpdateCancellationSetting(cancellationRefundRule, policyDuration, lastNumberOfYearsWhichNoClaimsMade);
            this.productFeatureSettingService.Setup(e => e.GetProductFeature(this.tenant.Id, this.product.Id)).Returns(productFeature);
        }

        private CachingJObjectWrapper GetFormData()
        {
            var formModel = $@"{{
                  ""formModel"": {{
                  }}
                }}";
            return new CachingJObjectWrapper(formModel);
        }

        private CachingJObjectWrapper GetCalculationData(bool provideRefund)
        {
            var calculationModel = string.Empty;
            if (provideRefund)
            {
                calculationModel = @"{
                                        ""questions"": {
                                            ""cancellation"": {
                                                ""provideRefund"": true
                                            }
                                        }
                                    }";
            }
            else
            {
                calculationModel = @"{
                                        ""questions"": {
                                            ""cancellation"": {
                                                ""provideRefund"": false
                                            }
                                        }
                                    }";
            }

            return new CachingJObjectWrapper(calculationModel);
        }

        private IPolicyService GetPolicyService(IProductFeatureSettingService productFeatureSettingService, IClaimReadModelRepository claimReadModelRepository)
        {
            var mockTenantRepository = new Mock<ITenantRepository>();
            var tenant = new Tenant(TenantFactory.DefaultId, "foo", "bar", null, default, default, this.clock.Now());
            mockTenantRepository.Setup(e => e.GetTenantById(TenantFactory.DefaultId)).Returns(tenant);

            var mockcachingResolver = new Mock<ICachingResolver>();
            var mockProductRepository = new Mock<IProductRepository>();
            var product = new Product(TenantFactory.DefaultId, ProductFactory.DefaultId, "bar", "bar", this.clock.Now());
            mockProductRepository.Setup(e => e.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockcachingResolver.Setup(e => e.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(e => e.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(e => e.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(e => e.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();

            this.quoteAggregateResolverService.Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            var policyService = new PolicyService(
                this.quoteAggregateRepository.Object,
                this.clock,
                mockcachingResolver.Object,
                claimReadModelRepository,
                this.productConfigProvider,
                new Mock<IUniqueIdentifierService>().Object,
                new Mock<IQuoteDocumentReadModelRepository>().Object,
                this.numberRepository.Object,
                new Mock<IPolicyReadModelRepository>().Object,
                this.systemAlertService,
                new Mock<IQuoteReferenceNumberGenerator>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                productFeatureSettingService,
                new Mock<IQuoteWorkflowProvider>().Object,
                new Mock<IPolicyTransactionTimeOfDayScheme>().Object,
                new Mock<IUBindDbContext>().Object);
            return policyService;
        }
    }
}
