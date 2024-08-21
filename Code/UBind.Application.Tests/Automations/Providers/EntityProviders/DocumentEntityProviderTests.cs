// <copyright file="DocumentEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain.Services;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class DocumentEntityProviderTests
    {
        [Fact]
        public async Task DocumentEntityProvider_Should_Return_DocumentEntity_When_Pass_With_DocumentId()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var json = @"{ 
                              ""documentId"" : """ + documentId.ToString() + @"""
                         }";

            var documentEntityProviderBuilder = JsonConvert.DeserializeObject<DocumentEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();

            var model = new DocumentReadModelWithRelatedEntities() { Document = new FakeQuoteDocumentReadModel(documentId) };
            mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteDocumentReadModelRepository))).Returns(mockDocumentRepository.Object);
            var documentEntityProvider = documentEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await documentEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Document));

            var quoteEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Document;
            quoteEntity.Id.Should().Be(documentId.ToString());
        }

        [Fact]
        public async Task DocumentEntityProvider_Should_Throw_When_EmailId_DoesNot_Exists()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var json = @"{ 
                              ""documentId"" : """ + documentId.ToString() + @"""
                         }";

            var documentEntityProviderBuilder = JsonConvert.DeserializeObject<DocumentEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();

            mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IDocumentReadModelWithRelatedEntities));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteDocumentReadModelRepository))).Returns(mockDocumentRepository.Object);
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            var documentEntityProvider = documentEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await documentEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Document");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Document ID: {documentId}");
        }
    }
}
