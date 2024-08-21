// <copyright file="IssuePolicyActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Actions;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NodaTime;
using System.Net;
using UBind.Application.Automation;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation.Providers.Entity;
using UBind.Application.Releases;
using UBind.Application.Services.Imports;
using UBind.Application.Tests.Automations.Fakes;
using UBind.Application.Tests.Automations.Providers.Fakes;
using UBind.Domain;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.Product.Component;
using UBind.Domain.ReadModel;
using UBind.Domain.ReferenceNumbers;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Tests.Fakes;
using Xunit;

public class IssuePolicyActionTests
{
    private ServiceCollection serviceCollection = new ServiceCollection();

    [Fact]
    public async void IssuePolicyAction_ShouldThrow_WhenPolicyAlreadyIssued()
    {
        // Arrange
        var mockQuoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        var quote = QuoteFactory.CreateQuoteWithPolicyIssued(
            TenantFactory.DefaultId,
            ProductFactory.DefaultId,
            DeploymentEnvironment.Development);
        mockQuoteAggregateResolverService
            .Setup(c => c.GetQuoteAggregateForQuote(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(quote.Aggregate);
        this.serviceCollection.AddScoped(c => mockQuoteAggregateResolverService.Object);
        var mockServiceProvider = this.GetServiceProvider(quote.Id);
        var issuePolicyActionBuilder = JsonConvert.DeserializeObject<IssuePolicyActionConfigModel>(
            TestJson.ActionConfig, AutomationDeserializationConfiguration.ModelSettings);
        var action = issuePolicyActionBuilder!.Build(mockServiceProvider);
        var actionData = new IssuePolicyActionData(action.Name, action.Alias, new TestClock());
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            TenantFactory.DefaultId,
            Guid.NewGuid(),
            ProductFactory.DefaultId,
            Domain.DeploymentEnvironment.Development,
            false,
            $"?quoteId={quote.Id}&policyNumber=QWE-202308071919&productId=UB-10029");

        // Act
        Func<Task> act = async () => await action.Execute(new ProviderContext(automationData), actionData);

        // Assert
        var exception = await act.Should().ThrowAsync<ErrorException>();
        exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
        exception.Which.Error.Code.Should().Be("policy.issuance.quote.already.has.policy");
    }

    [Fact]
    public async void IssuePolicyAction_ShouldThrow_WhenQuoteNotInAllowedState()
    {
        // Arrange
        var mockQuoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        var quote = QuoteFactory.CreateNewBusinessQuote(
            TenantFactory.DefaultId,
            ProductFactory.DefaultId,
            DeploymentEnvironment.Development);
        quote.Aggregate.WithCalculationResult(quote.Id);
        mockQuoteAggregateResolverService
            .Setup(c => c.GetQuoteAggregateForQuote(TenantFactory.DefaultId, It.IsAny<Guid>())).Returns(quote.Aggregate);
        this.serviceCollection.AddScoped(c => mockQuoteAggregateResolverService.Object);
        var mockServiceProvider = this.GetServiceProvider(quote.Id);
        var issuePolicyActionBuilder = JsonConvert.DeserializeObject<IssuePolicyActionConfigModel>(
            TestJson.ActionConfig, AutomationDeserializationConfiguration.ModelSettings);
        var action = issuePolicyActionBuilder!.Build(mockServiceProvider);
        var actionData = new IssuePolicyActionData(action.Name, action.Alias, new TestClock());
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            TenantFactory.DefaultId,
            Guid.NewGuid(),
            ProductFactory.DefaultId,
            Domain.DeploymentEnvironment.Development,
            false,
            $"?quoteId={quote.Id}&policyNumber=QWE-202308071919&productId=UB-10029");

        // Act
        Func<Task> act = async () => await action.Execute(new ProviderContext(automationData), actionData);

        // Assert
        var exception = await act.Should().ThrowAsync<ErrorException>();
        exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        exception.Which.Error.Code.Should().Be("policy.issuance.quote.state.invalid");
    }

    private IServiceProvider GetServiceProvider(Guid entityId)
    {
        var mockCachingResolver = new Mock<ICachingResolver>();
        var product = ProductFactory.Create(TenantFactory.DefaultId, ProductFactory.DefaultId);
        mockCachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
        mockCachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).ReturnsAsync(product);
        this.serviceCollection.AddScoped(x => mockCachingResolver.Object);

        var mockProductService = new Mock<IProductService>();
        mockProductService
            .Setup(x => x.ThrowIfProductNotEnabledForOrganisation(product, It.IsAny<Guid>())).Returns(Task.CompletedTask);
        mockProductService
            .Setup(x => x.ThrowIfNewBusinessPolicyTransactionsDisabled(product));
        this.serviceCollection.AddSingleton(mockProductService.Object);

        var mockProductRepository = new Mock<IProductRepository>();
        var productModel = new ProductWithRelatedEntities
        {
            Product = product,
        };
        mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(productModel);
        this.serviceCollection.AddScoped(c => mockProductRepository.Object);

        var mockReleaseQueryService = new Mock<IReleaseQueryService>();
        var mockdevReleaseRepository = new Mock<IDevReleaseRepository>();
        var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(Guid.NewGuid(), Guid.NewGuid())
                .BuildDevRelease();
        var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, DeploymentEnvironment.Development, new FieldSerializationBinder());
        mockReleaseQueryService.Setup(e => e.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);
        mockdevReleaseRepository.Setup(e => e.GetDevReleaseForProductWithoutAssets(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(fakeDevRelease);
        this.serviceCollection.AddScoped(c => mockReleaseQueryService.Object);
        this.serviceCollection.AddScoped(c => mockdevReleaseRepository.Object);

        var mockMediator = new Mock<ICqrsMediator>();
        this.serviceCollection.AddSingleton(mockMediator.Object);
        var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
        var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
        var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
        mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
        this.serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
            mockUrlConfiguration.Object,
            mockProductConfigProvider.Object,
            mockFormDataPrettifier.Object,
            mockCachingResolver.Object,
            mockMediator.Object,
            new DefaultPolicyTransactionTimeOfDayScheme()));

        var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
        var quoteModel = new QuoteReadModelWithRelatedEntities();
        quoteModel.Quote = new FakeNewQuoteReadModel(entityId);
        quoteModel.Quote.QuoteState = QuoteStatus.Discarded.ToString();
        quoteModel.PolicyTransaction = new FakePolicyTransaction(TenantFactory.DefaultId, entityId);
        quoteModel.Policy = new FakePolicyReadModel(TenantFactory.DefaultId, entityId);
        mockQuoteRepository
            .Setup(c => c.GetQuoteWithRelatedEntities(
                It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
            .Returns(quoteModel);
        mockQuoteRepository
            .Setup(c => c.GetQuoteWithRelatedEntities(
                It.IsAny<Guid>(), null, It.IsAny<Guid>(), It.IsAny<List<string>>()))
            .Returns(quoteModel);
        this.serviceCollection.AddScoped(c => mockQuoteRepository.Object);

        this.serviceCollection.AddScoped<IClock>(c => new TestClock());
        var quoteWorkflowProvider = new Mock<IQuoteWorkflowProvider>();
        this.serviceCollection.AddScoped(q => quoteWorkflowProvider.Object);
        quoteWorkflowProvider.Setup(provide => provide.GetConfigurableQuoteWorkflow(It.IsAny<ReleaseContext>()))
            .Returns(Task.FromResult(new DefaultQuoteWorkflow() as IQuoteWorkflow));
        var mockPolicyNumberRepository = new Mock<IPolicyNumberRepository>();
        mockPolicyNumberRepository
                .Setup(s => s.GetAvailableForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(new List<string> { "INV00012", "INV00013" });
        mockPolicyNumberRepository
                .Setup(s => s.ConsumeAndSave(It.IsAny<ProductContext>()))
                .Returns("INV00012");
        this.serviceCollection.AddScoped(q => mockPolicyNumberRepository.Object);
        this.serviceCollection.AddLoggers();
        this.serviceCollection.AddScoped(q => new Mock<IProductConfigurationProvider>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IHttpContextPropertiesResolver>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IPolicyService>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IQuoteAggregateRepository>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IPersonAggregateRepository>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IAdditionalPropertyValueService>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IAdditionalPropertyTransformHelper>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IPolicyTransactionTimeOfDayScheme>().Object);
        this.serviceCollection.AddScoped(q => new Mock<IUBindDbContext>().Object);

        return this.serviceCollection.BuildServiceProvider();
    }

    private class TestJson
    {
        public static string ActionConfig => @"
            {
                ""name"": ""Issue Policy Action Using Quote"",
                ""alias"": ""issuePolicyActionUsingQuote"",
                ""quote"": {
                    ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/quoteId""
                },
                ""policyNumber"": {
                    ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/policyNumber""
                },
                ""product"": {
                    ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/productId""
                }
            }";
    }
}
