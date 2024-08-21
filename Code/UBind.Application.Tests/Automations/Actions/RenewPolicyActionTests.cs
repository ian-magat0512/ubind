// <copyright file="RenewPolicyActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Actions;

using System.Threading.Tasks;
using Moq;
using Xunit;
using NodaTime;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Services;
using UBind.Domain.Exceptions;
using UBind.Application.Releases;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.ReferenceNumbers;
using FluentAssertions;
using UBind.Application.Tests.Automations.Fakes;

public class RenewPolicyActionTests
{
    private readonly Mock<ICqrsMediator> mockMediator;
    private readonly Mock<IClock> mockClock;
    private readonly Mock<IReleaseQueryService> mockReleaseQueryService;
    private readonly Mock<ICachingResolver> mockCachingResolver;
    private readonly Mock<IQuoteAggregateResolverService> mockQuoteAggregateResolverService;
    private readonly Mock<IProductConfigurationProvider> mockProductConfigurationProvider;
    private readonly Mock<IQuoteWorkflowProvider> mockQuoteWorkflowProvider;
    private readonly Mock<IHttpContextPropertiesResolver> mockHttpContextPropertiesResolver;
    private readonly Mock<IPolicyService> mockPolicyService;
    private readonly Mock<IQuoteAggregateRepository> mockQuoteAggregateRepository;
    private readonly Mock<IPolicyTransactionTimeOfDayScheme> mockTimeOfDayScheme;
    private readonly Mock<IInvoiceNumberRepository> mockInvoiceNumberRepository;
    private readonly Mock<IProductFeatureSettingService> mockProductFeatureSettingService;

    public RenewPolicyActionTests()
    {
        this.mockMediator = new Mock<ICqrsMediator>();
        this.mockClock = new Mock<IClock>();
        this.mockReleaseQueryService = new Mock<IReleaseQueryService>();
        this.mockCachingResolver = new Mock<ICachingResolver>();
        this.mockQuoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        this.mockProductConfigurationProvider = new Mock<IProductConfigurationProvider>();
        this.mockQuoteWorkflowProvider = new Mock<IQuoteWorkflowProvider>();
        this.mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        this.mockPolicyService = new Mock<IPolicyService>();
        this.mockQuoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        this.mockTimeOfDayScheme = new Mock<IPolicyTransactionTimeOfDayScheme>();
        this.mockInvoiceNumberRepository = new Mock<IInvoiceNumberRepository>();
        this.mockProductFeatureSettingService = new Mock<IProductFeatureSettingService>();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange + Act
        var action = new RenewPolicyAction(
            "TestAction",
            "TestAlias",
            "TestDescription",
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            this.mockMediator.Object,
            this.mockClock.Object,
            this.mockProductConfigurationProvider.Object,
            this.mockQuoteWorkflowProvider.Object,
            this.mockHttpContextPropertiesResolver.Object,
            this.mockCachingResolver.Object,
            this.mockQuoteAggregateResolverService.Object,
            this.mockPolicyService.Object,
            this.mockReleaseQueryService.Object,
            this.mockProductFeatureSettingService.Object,
            this.mockQuoteAggregateRepository.Object,
            this.mockInvoiceNumberRepository.Object,
            this.mockTimeOfDayScheme.Object);

        // Assert
        action.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_ShouldThrowErrorException_WhenQuoteAndPolicyNotProvided()
    {
        // Arrange
        var action = new RenewPolicyAction(
            "TestAction",
            "TestAlias",
            "TestDescription",
            true,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            this.mockMediator.Object,
            this.mockClock.Object,
            this.mockProductConfigurationProvider.Object,
            this.mockQuoteWorkflowProvider.Object,
            this.mockHttpContextPropertiesResolver.Object,
            this.mockCachingResolver.Object,
            this.mockQuoteAggregateResolverService.Object,
            this.mockPolicyService.Object,
            this.mockReleaseQueryService.Object,
            this.mockProductFeatureSettingService.Object,
            this.mockQuoteAggregateRepository.Object,
            this.mockInvoiceNumberRepository.Object,
            this.mockTimeOfDayScheme.Object);

        var mockData = await MockAutomationData.CreateWithHttpTrigger();
        var providerContext = new ProviderContext(mockData);
        var actionData = new RenewPolicyActionData("TestName", "TestAlias", this.mockClock.Object);

        // Act + Assert
        var exception = await Assert.ThrowsAsync<ErrorException>(() => action.Execute(providerContext, actionData));

        exception.Error.Code.Should().Be("policy.renewal.quote.and.policy.not.provided");
    }
}
