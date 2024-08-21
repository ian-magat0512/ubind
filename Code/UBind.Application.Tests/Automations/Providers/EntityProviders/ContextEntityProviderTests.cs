// <copyright file="ContextEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntity = UBind.Domain.SerialisedEntitySchemaObject;

    [SystemEventTypeExtensionInitialize]
    public class ContextEntityProviderTests
    {
        private Guid quoteId = new Guid("1d20a4f4-ddaa-49cc-b99b-09c4b3c65ecc");

        [Fact]
        public async Task ContextEntityProvider_ReturnsEntityReference_IfExistsInAutomationData()
        {
            // Arrange
            var pathProvider = new StaticBuilder<Data<string>>() { Value = "/quote" };
            var contextEntityProviderBuilder = new ContextEntityProviderConfigModel() { Path = pathProvider };
            var automationData = this.CreateAutomationData();
            var serviceProvider = this.GetServiceProvider();
            var contextEntityProvider = contextEntityProviderBuilder.Build(serviceProvider);

            // Act
            var entity = (await contextEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entity.DataValue.Should().NotBeNull();
            entity.DataValue
                .Should()
                .BeOfType(typeof(SerialisedEntity.Quote));
            var quoteEntity = entity.DataValue as SerialisedEntity.Quote;
            quoteEntity.Id.Should().Be(this.quoteId.ToString());
        }

        [Fact]
        public async Task ContextEntityProvider_ThrowsAnError_WhenReferenceIsNotInAutomationDataAsync()
        {
            // Arrange
            var pathProvider = new StaticBuilder<Data<string>>() { Value = "/customer" };
            var contextEntityProviderBuilder = new ContextEntityProviderConfigModel() { Path = pathProvider };
            var automationData = this.CreateAutomationData();
            var serviceProvider = this.GetServiceProvider();
            var contextEntityProvider = contextEntityProviderBuilder.Build(serviceProvider);

            // Act
            Func<Task> func = async () => await contextEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("automation.providers.entity.context.entity.not.found");
        }

        public MockAutomationData CreateAutomationData()
        {
            var automationData = MockAutomationData.CreateWithEventTrigger();
            List<Relationship> dummyRelationships = new List<Relationship>()
            {
                new Relationship(
                    Guid.Empty,
                    EntityType.Quote,
                    this.quoteId,
                    RelationshipType.QuoteEvent,
                    EntityType.Quote,
                    new Guid("80e84e48-5e77-4c73-9310-7a4cc60934ac"),
                    SystemClock.Instance.GetCurrentInstant()),
            };
            automationData.ContextManager.SetContextFromEventRelationships(dummyRelationships);
            automationData.SetServiceProvider(this.GetServiceProvider());
            return automationData;
        }

        private IServiceProvider GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(this.quoteId) };

            var mockCachingResolver = new Mock<ICachingResolver>();
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(TenantFactory.Create()));
            mockCachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(ProductFactory.Create()));

            var mockQuoteReadModelRepo = new Mock<IQuoteReadModelRepository>();
            mockQuoteReadModelRepo
                .Setup(x => x.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), this.quoteId, new List<string>()))
                .Returns(model);
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository)))
                .Returns(mockQuoteReadModelRepo.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(mockCachingResolver.Object);

            return MockAutomationData.GetServiceProviderForEntityProviders(this.quoteId, false);
        }
    }
}
