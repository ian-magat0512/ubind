// <copyright file="GetContextEntitiesQueryHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.ContextEntity
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Queries.ContextEntity;
    using UBind.Application.Releases;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using Xunit;
    using IClock = NodaTime.IClock;

    public class GetContextEntitiesQueryHandlerTests
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Mock<IQuoteReadModelRepository> quoteReadModelRepository;
        private readonly Mock<IClaimReadModelRepository> claimReadModelRepository;
        private readonly Mock<IReleaseQueryService> releaseQueryServiceMock;
        private readonly Mock<ISerialisedEntityFactory> serialisedEntityFactoryMock;
        private readonly Mock<ICachingResolver> cachingResolverMock;
        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock;
        private readonly GetContextEntitiesQueryHandler handler;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public GetContextEntitiesQueryHandlerTests()
        {
            this.quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
            this.claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            this.releaseQueryServiceMock = new Mock<IReleaseQueryService>();
            this.serialisedEntityFactoryMock = new Mock<ISerialisedEntityFactory>();
            this.cachingResolverMock = new Mock<ICachingResolver>();
            this.httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();

            var internalUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            internalUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(c => this.quoteReadModelRepository.Object);
            serviceCollection.AddScoped(c => this.claimReadModelRepository.Object);
            serviceCollection.AddSingleton<IClock>(c => SystemClock.Instance);
            serviceCollection.AddSingleton(x => internalUrlConfiguration.Object);
            serviceCollection.AddScoped(c => this.cachingResolverMock.Object);
            serviceCollection.AddScoped(c => this.serialisedEntityFactoryMock.Object);
            serviceCollection.AddScoped(serviceCollection => this.httpContextPropertiesResolverMock.Object);

            this.serviceProvider = serviceCollection.BuildServiceProvider();

            this.handler = new GetContextEntitiesQueryHandler(
                this.serviceProvider,
                this.quoteReadModelRepository.Object,
                this.claimReadModelRepository.Object,
                this.releaseQueryServiceMock.Object,
                this.cachingResolverMock.Object,
                this.httpContextPropertiesResolverMock.Object);
        }

        [Fact]
        public async void Handle_ReturnsQuoteWithRelatedEntities_ForQuoteWebFormApp()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var releaseContext = new ReleaseContext(Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development, Guid.NewGuid());
            var request = new GetContextEntitiesQuery(
                releaseContext, Guid.NewGuid(), quoteId, WebFormAppType.Quote, QuoteType.NewBusiness);
            var fakeDevRelease = FakeReleaseBuilder
                    .CreateForProduct(Guid.NewGuid(), Guid.NewGuid())
                    .WithQuoteFormConfiguration(this.GetQuoteFormConfigurationJson())
                    .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, DeploymentEnvironment.Development, null);
            this.releaseQueryServiceMock.Setup(e => e.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);

            // Act
            var result = await this.handler.Handle(request, CancellationToken.None);

            // Asssert
            this.quoteReadModelRepository.Verify(q => q.GetQuoteWithRelatedEntities(
                request.ReleaseContext.TenantId, request.ReleaseContext.Environment, quoteId, It.IsAny<string[]>()));
            result.Should().NotBeNull();
        }

        [Fact]
        public async void Handle_ReturnsClaimWithRelatedEntities_ForClaimWebFormApp()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var releaseContext = new ReleaseContext(Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development, Guid.NewGuid());
            var request = new GetContextEntitiesQuery(
                releaseContext, Guid.NewGuid(), claimId, WebFormAppType.Claim);
            var fakeDevRelease = FakeReleaseBuilder
                    .CreateForProduct(Guid.NewGuid(), Guid.NewGuid())
                    .WithClaimFormConfiguration(this.GetClaimFormConfigurationJson())
                    .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, DeploymentEnvironment.Development, null);
            this.releaseQueryServiceMock.Setup(e => e.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);

            // Act
            var result = await this.handler.Handle(request, CancellationToken.None);

            // Asssert
            this.claimReadModelRepository.Verify(c => c.GetClaimWithRelatedEntities(
                request.ReleaseContext.TenantId, request.ReleaseContext.Environment, claimId, It.IsAny<string[]>()));
            result.Should().NotBeNull();
        }

        private string GetQuoteFormConfigurationJson()
        {
            var component = new Component();
            component.ContextEntities = new ContextEntities();
            component.ContextEntities.Quotes = new ContextEntitySettings();
            component.ContextEntities.Quotes.IncludeContextEntities = new string[]
            {
                "/quote",
                "/claim",
            };

            return JsonConvert.SerializeObject(component, this.serializerSettings);
        }

        private string GetClaimFormConfigurationJson()
        {
            var component = new Component();
            component.ContextEntities = new ContextEntities();
            component.ContextEntities.Claims = new ContextEntitySettings();
            component.ContextEntities.Claims.IncludeContextEntities = new string[]
            {
                "/quote",
                "/claim",
            };
            return JsonConvert.SerializeObject(component, this.serializerSettings);
        }
    }
}
