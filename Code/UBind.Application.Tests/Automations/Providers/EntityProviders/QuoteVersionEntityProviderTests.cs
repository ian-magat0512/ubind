// <copyright file="QuoteVersionEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class QuoteVersionEntityProviderTests
    {
        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Return_QuoteVersionEntity_When_Pass_With_QuoteId_And_Version()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();
            var json = @"{ 
                              ""quoteId"" : """ + quoteId.ToString() + @""",
                              ""versionNumber"" : " + quoteVersion + @"
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await quoteVersionEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.QuoteVersion));

            var quoteVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.QuoteVersion;
            quoteVersionEntity.Id.Should().Be(expectedQuoteVersionId.ToString());
        }

        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Return_QuoteVersionEntity_When_Pass_With_QuoteReference_And_Version()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();

            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @""",
                              ""versionNumber"" : """ + quoteVersion + @"""
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await quoteVersionEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.QuoteVersion));

            var quoteVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.QuoteVersion;
            quoteVersionEntity.Id.Should().Be(expectedQuoteVersionId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockQuoteVersionRepository
                .Verify(c => c.GetQuoteVersionWithRelatedEntities(It.IsAny<Guid>(), quoteReference, DeploymentEnvironment.Production, quoteVersion, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Return_QuoteVersionEntity_When_Pass_With_QuoteReference_And_Version_With_StringValue()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();

            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @""",
                              ""versionNumber"" : 
                                {
                                    ""parseTextInteger"" : 
                                    {
                                        ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/version""
                                    }
                                }
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(queryParams: "?version=1", environment: DeploymentEnvironment.Production);

            // Act
            var entityObject = (await quoteVersionEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.QuoteVersion));

            var quoteVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.QuoteVersion;
            quoteVersionEntity.Id.Should().Be(expectedQuoteVersionId.ToString());

            // Check if the mock quote repository uses product context environment.
            mockQuoteVersionRepository
                .Verify(c => c.GetQuoteVersionWithRelatedEntities(It.IsAny<Guid>(), quoteReference, DeploymentEnvironment.Production, quoteVersion, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Return_QuoteVersionEntity_When_Pass_With_QuoteReference_And_Environment()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();

            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @""",
                              ""versionNumber"" : """ + quoteVersion + @""",
                              ""environment"" : ""Development""
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await quoteVersionEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.QuoteVersion));

            var quoteVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.QuoteVersion;
            quoteVersionEntity.Id.Should().Be(expectedQuoteVersionId.ToString());

            // Check if the mock quote repository uses the passed environment.
            mockQuoteVersionRepository
                .Verify(c => c.GetQuoteVersionWithRelatedEntities(It.IsAny<Guid>(), quoteReference, DeploymentEnvironment.Development, quoteVersion, It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Throw_When_QuoteId_DoesNot_Exists()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var quoteVersion = 1;
            var json = @"{ 
                              ""quoteId"" : """ + quoteId.ToString() + @""",
                              ""versionNumber"" : """ + quoteVersion + @"""
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(default(IQuoteVersionReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await quoteVersionEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Quote Version");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Quote ID: {quoteId}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Version Number: 1");
        }

        [Fact]
        public async Task QuoteVersionEntityProvider_Should_Throw_When_QuoteReference_DoesNot_Exists()
        {
            // Arrange
            var quoteReference = "NKHSA";
            var quoteVersion = 1;
            var json = @"{ 
                              ""quoteReference"" : """ + quoteReference.ToString() + @""",
                              ""versionNumber"" : """ + quoteVersion + @"""
                         }";

            var quoteVersionEntityProviderBuilder = JsonConvert.DeserializeObject<QuoteVersionEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(default(IQuoteVersionReadModelWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var quoteVersionEntityProvider = quoteVersionEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await quoteVersionEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Quote Version");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Quote Reference: {quoteReference}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Version Number: 1");
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
