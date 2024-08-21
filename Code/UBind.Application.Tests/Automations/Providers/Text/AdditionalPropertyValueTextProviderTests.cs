// <copyright file="AdditionalPropertyValueTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
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
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdditionalPropertyValueTextProviderTests
    {
        private readonly Guid quoteId = Guid.NewGuid();
        private readonly string propAlias = "prop-one";
        private readonly string propAliasValue = "Prop One Value";
        private readonly string defaultValue = "The Default Value";

        private AdditionalPropertyValueDto propAliasDtoValue;

        [Fact]
        public async Task AdditionalPropertyValueTextProvider_Should_ReturnCorrectPropertyTextValue()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            this.propAliasDtoValue = new AdditionalPropertyValueDto
            {
                AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                {
                    Id = Guid.NewGuid(),
                },
                Value = this.propAliasValue,
            };

            var json = @"{
                    ""entity"": {
                        ""quote"": {
                            ""quoteId"": """ + this.quoteId.ToString() + @"""
                        },
                    },
                    ""propertyAlias"": """ + this.propAlias + @"""
                }";
            var additionalPropertyValueTextProviderBuilder =
                JsonConvert.DeserializeObject<AdditionalPropertyValueTextProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider();

            var additionalPropertyValueTextProvider =
                additionalPropertyValueTextProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var additionalPropertyValueText = (await additionalPropertyValueTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            additionalPropertyValueText.DataValue.Should().Be(this.propAliasValue);
        }

        [Fact]
        public async Task AdditionalPropertyValueTextProvider_Should_ReturnDefaultValueWhenNoValueFound()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                    ""entity"": {
                        ""quote"": {
                            ""quoteId"": """ + this.quoteId.ToString() + @"""
                        },
                    },
                    ""propertyAlias"": """ + this.propAlias + @"""
                }";
            var additionalPropertyValueTextProviderBuilder =
                JsonConvert.DeserializeObject<AdditionalPropertyValueTextProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider();

            var additionalPropertyValueTextProvider =
                additionalPropertyValueTextProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var additionalPropertyValueText = (await additionalPropertyValueTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            additionalPropertyValueText.DataValue.Should().Be(this.defaultValue);
        }

        [Fact]
        public async Task AdditionalPropertyValueTextProvider_Should_ThrowError_WhenPropertyAliasDoesNotExists()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            this.propAliasDtoValue = new AdditionalPropertyValueDto
            {
                AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                {
                    Id = Guid.NewGuid(),
                },
                Value = this.propAliasValue,
            };

            var json = @"{
                    ""entity"": {
                        ""quote"": {
                            ""quoteId"": """ + this.quoteId.ToString() + @"""
                        },
                    },
                    ""propertyAlias"": ""not-exists""
                }";
            var additionalPropertyValueTextProviderBuilder =
                JsonConvert.DeserializeObject<AdditionalPropertyValueTextProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider();

            var additionalPropertyValueTextProvider =
                additionalPropertyValueTextProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            Func<Task> func = async () => await additionalPropertyValueTextProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("read.additional.property.alias.invalid");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();

            var mockCachingResolver = new Mock<ICachingResolver>();
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(new Tenant(TenantFactory.DefaultId)));

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockTextAdditionalPropertyValueReadModelRepository = new Mock<ITextAdditionalPropertyValueReadModelRepository>();
            mockTextAdditionalPropertyValueReadModelRepository.Setup(
                c => c.GetAdditionalPropertyValueByEntityIdAndPropertyAlias(
                    It.IsAny<Guid>(),
                    this.quoteId,
                    this.propAlias)).Returns(this.propAliasDtoValue);

            var mockaAdditionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>();
            mockaAdditionalPropertyDefinitionRepository.Setup(
                c => c.DoesEntityAdditionalPropertyDefinitionsContainPropertyAlias(It.IsAny<Guid>(), this.propAlias, It.IsAny<AdditionalPropertyEntityType>())).Returns(true);
            mockaAdditionalPropertyDefinitionRepository.Setup(
                c => c.GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(
                    It.IsAny<Guid>(),
                    this.propAlias,
                    It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(
                    new AdditionalPropertyDefinitionReadModel(
                        TenantFactory.DefaultId,
                        default,
                        default,
                        default,
                        default,
                        default,
                        default,
                        default,
                        false,
                        false,
                        false,
                        this.defaultValue,
                        default,
                        default));

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(this.quoteId) };
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<List<string>>()))
                .Returns(model);
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(mockCachingResolver.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository)))
                .Returns(mockQuoteRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
               .Returns(new SerialisedEntityFactory(
                   mockUrlConfiguration.Object,
                   mockProductConfigProvider.Object,
                   mockFormDataPrettifier.Object,
                   mockCachingResolver.Object,
                   mockMediator.Object,
                   new DefaultPolicyTransactionTimeOfDayScheme()));
            mockServiceProvider.Setup(c => c.GetService(typeof(ITextAdditionalPropertyValueReadModelRepository)))
                .Returns(mockTextAdditionalPropertyValueReadModelRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(IAdditionalPropertyDefinitionRepository)))
                .Returns(mockaAdditionalPropertyDefinitionRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(IStructuredDataAdditionalPropertyValueReadModelRepository)))
                 .Returns(new Mock<IStructuredDataAdditionalPropertyValueReadModelRepository>().Object);

            return mockServiceProvider;
        }
    }
}
