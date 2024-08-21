// <copyright file="QuoteEntityProviderTests.cs" company="uBind">
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
    using Newtonsoft.Json;
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
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using Xunit;

    public class QuoteEntityProviderTests
    {
        [Fact]
        public async Task QuoteEntityProvider_Should_Return_QuoteEntity_When_Pass_With_QuoteId()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{ 
                              ""quoteId"" : """ + quoteId.ToString() + @"""
                         }";

            var quoteEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(quoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var quoteEntityProvider = quoteEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await quoteEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(Quote));

            var quoteEntity = entityObject.DataValue as Quote;
            quoteEntity.Id.Should().Be(quoteId.ToString());
        }

        [Fact]
        public async Task QuoteEntityProvider_Should_Return_QuoteEntity_When_Pass_With_QuoteReference()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var expectedQuoteId = Guid.NewGuid();

            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @"""
                         }";

            var quoteEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(expectedQuoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var quoteEntityProvider = quoteEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await quoteEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(Quote));

            var quoteEntity = entityObject.DataValue as Quote;
            quoteEntity.Id.Should().Be(expectedQuoteId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockQuoteRepository
                .Verify(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), quoteReference, DeploymentEnvironment.Production, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task QuoteEntityProvider_Should_Return_QuoteEntity_When_Pass_With_QuoteReference_And_Environment()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var expectedQuoteId = Guid.NewGuid();

            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @""",
                              ""environment"" : ""Development""
                         }";

            var quoteEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(expectedQuoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var quoteEntityProvider = quoteEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await quoteEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(Quote));

            var quoteEntity = entityObject.DataValue as Quote;
            quoteEntity.Id.Should().Be(expectedQuoteId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockQuoteRepository
                .Verify(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), quoteReference, DeploymentEnvironment.Development, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task QuoteEntityProvider_Should_Throw_When_QuoteId_DoesNot_Exists()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{ 
                              ""quoteId"" : """ + quoteId.ToString() + @"""
                         }";

            var quoteEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IQuoteReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var quoteEntityProvider = quoteEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await quoteEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Quote");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Quote ID: {quoteId}");
        }

        [Fact]
        public async Task QuoteEntityProvider_Should_Throw_When_QuoteReference_DoesNot_Exists()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @"""
                         }";

            var quoteEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<List<string>>()))
                .Returns(default(IQuoteReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var quoteEntityProvider = quoteEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await quoteEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Quote");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Quote Number: {quoteReference}");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            return mockServiceProvider;
        }
    }
}
