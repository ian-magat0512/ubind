// <copyright file="IncrementAdditionalPropertyValueActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Queries.AdditionalPropertyDefinition;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class IncrementAdditionalPropertyValueActionTests
    {
        private readonly Guid quoteId = Guid.NewGuid();
        private readonly string propAlias = "prop-one";

        private string propAliasValue = string.Empty;
        private AdditionalPropertyValueDto propAliasDtoValue;

        [Fact]
        public async void IncrementAdditionalPropertyValueAction_Should_Increment_WhenPropertyValueCanBeParsedAsInteger()
        {
            // Arrange
            this.propAliasValue = "1";
            this.propAliasDtoValue = new AdditionalPropertyValueDto
            {
                AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                {
                    Id = Guid.NewGuid(),
                    Alias = this.propAlias,
                },
                Value = this.propAliasValue,
            };
            var json = @"{
                            ""name"": ""Increment AdditionalProperty Value Action"",
                            ""alias"": ""incrementAdditionalPropertyValueAction"",
                            ""entity"": {
                                ""dynamicEntity"": 
                                {
                                    ""entityType"": ""quote"",
                                    ""entityId"": """ + this.quoteId.ToString() + @"""
                                }
                            },
                            ""propertyAlias"": """ + this.propAlias + @"""
                        }";
            var incrementAdditionalPropertyValueActionBuilder = JsonConvert.DeserializeObject<IncrementAdditionalPropertyValueActionConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = this.GetServiceProvider(this.quoteId);
            var incrementAdditionalPropertyValueAction = incrementAdditionalPropertyValueActionBuilder.Build(mockServiceProvider);
            var actionData = new IncrementAdditionalPropertyValueActionData(incrementAdditionalPropertyValueAction.Name, incrementAdditionalPropertyValueAction.Alias, new TestClock());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var expectedOutput = $"{int.Parse(this.propAliasValue) + 1}";

            // Act
            await incrementAdditionalPropertyValueAction.Execute(
                new ProviderContext(automationData),
                actionData);

            // Assert
            actionData.EntityId.Should().Be(this.quoteId.ToString());
            actionData.EntityType.ToLowerInvariant().Should().Be("quote");
            actionData.PreviousValue.Should().Be(this.propAliasValue);
            actionData.ResultingValue.Should().Be(expectedOutput);
        }

        [Fact]
        public async Task IncrementAdditionalPropertyValueAction_Should_Throw_WhenPropertyValueIsNotAnInteger()
        {
            // Arrange
            this.propAliasValue = "one";
            this.propAliasDtoValue = new AdditionalPropertyValueDto
            {
                AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                {
                    Id = Guid.NewGuid(),
                    Alias = this.propAlias,
                },
                Value = this.propAliasValue,
            };
            var json = @"{
                            ""name"": ""Increment AdditionalProperty Value Action"",
                            ""alias"": ""incrementAdditionalPropertyValueAction"",
                            ""entity"": {
                                ""dynamicEntity"": 
                                {
                                    ""entityType"": ""quote"",
                                    ""entityId"": """ + this.quoteId.ToString() + @"""
                                }
                            },
                            ""propertyAlias"": """ + this.propAlias + @"""
                        }";
            var incrementAdditionalPropertyValueActionBuilder = JsonConvert.DeserializeObject<IncrementAdditionalPropertyValueActionConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = this.GetServiceProvider(this.quoteId);
            var incrementAdditionalPropertyValueAction = incrementAdditionalPropertyValueActionBuilder.Build(mockServiceProvider);
            var actionData = new IncrementAdditionalPropertyValueActionData(incrementAdditionalPropertyValueAction.Name, incrementAdditionalPropertyValueAction.Alias, new TestClock());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> action = async () => await incrementAdditionalPropertyValueAction.Execute(
                 new ProviderContext(automationData),
                 actionData);

            // Assert
            var error = await action.Should().ThrowAsync<ErrorException>();
            error.And.Error.Title.Should().Be("Additional property value cannot be parsed as integer");
        }

        private IServiceProvider GetServiceProvider(Guid entityId)
        {
            var serviceCollection = new ServiceCollection();

            var mockTextAdditionalPropertyValueReadModelRepository = new Mock<ITextAdditionalPropertyValueReadModelRepository>();
            mockTextAdditionalPropertyValueReadModelRepository.Setup(
                c => c.GetAdditionalPropertyValueByEntityIdAndPropertyAlias(It.IsAny<Guid>(), this.quoteId, this.propAlias)).Returns(this.propAliasDtoValue);
            mockTextAdditionalPropertyValueReadModelRepository.Setup(
                c => c.DoesAdditionalPropertyValueExistsForEntityIdAndPropertyAlias(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyEntityType>(), this.propAlias, "3")).Returns(true);

            var mockaAdditionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>();
            mockaAdditionalPropertyDefinitionRepository.Setup(
                c => c.DoesEntityAdditionalPropertyDefinitionsContainPropertyAlias(It.IsAny<Guid>(), this.propAlias, It.IsAny<AdditionalPropertyEntityType>())).Returns(true);

            var definition = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            mockaAdditionalPropertyDefinitionRepository.Setup(
              c => c.GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(It.IsAny<Guid>(), this.propAlias, It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(definition);

            var properties = new List<AdditionalPropertyValueDto>() { this.propAliasDtoValue };
            var mockAddPropertyService = new Mock<IAdditionalPropertyValueService>();
            mockAddPropertyService.Setup(c => c.GetAdditionalPropertyValuesByEntityTypeAndEntityId(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyEntityType>(), It.IsAny<Guid>())).Returns(Task.FromResult(properties));

            var mockDbAdditionalPropertyValue = new AdditionalPropertyValueDto()
            {
                EntityId = this.quoteId,
                Value = int.TryParse(this.propAliasValue, out int parseResult) ? (parseResult + 1).ToString() : this.propAliasValue,
            };
            properties = new List<AdditionalPropertyValueDto>() { mockDbAdditionalPropertyValue };
            mockAddPropertyService
                .Setup(c => c.UpdateAdditionalPropertyValueForEntity(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<AdditionalPropertyDefinitionReadModel>(),
                    "2"))
                .Returns(Task.FromResult(properties));

            var mockAdditionalPropertyValueProcessor = new Mock<IAdditionalPropertyValueProcessor>();
            var request = new GetAdditionalPropertyValuesQuery(
                Guid.Empty,
                default(AdditionalPropertyEntityType),
                Guid.Empty,
                AdditionalPropertyDefinitionType.Text,
                this.propAliasDtoValue.AdditionalPropertyDefinition.Id,
                this.propAliasValue);
            mockAdditionalPropertyValueProcessor.Setup(c => c.GetAdditionalPropertyValues(It.IsAny<Guid>(), request)).Returns(Task.FromResult(properties));
            mockTextAdditionalPropertyValueReadModelRepository.Setup(
                c => c.GetAdditionalPropertyValuesBy(It.IsAny<Guid>(), It.IsAny<IAdditionalPropertyValueListFilter>())).Returns(Task.FromResult(properties));

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(this.quoteId) };
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            mockQuoteRepository.Setup(
                c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<List<string>>()))
                .Returns(model);
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), this.quoteId, It.IsAny<List<string>>()))
                .Returns(model);

            serviceCollection.AddScoped(c => mockAddPropertyService.Object);
            serviceCollection.AddScoped(c => mockTextAdditionalPropertyValueReadModelRepository.Object);
            serviceCollection.AddScoped(c => mockaAdditionalPropertyDefinitionRepository.Object);
            serviceCollection.AddScoped<PropertyTypeEvaluatorService>((sp) =>
            {
                var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
                {
                    {
                        AdditionalPropertyDefinitionType.Text,
                        new TextAdditionalPropertyValueProcessor(
                            sp.GetService<ITextAdditionalPropertyValueReadModelRepository>(),
                            sp.GetService<IClock>(),
                            sp.GetService<ITextAdditionalPropertyValueAggregateRepository>(),
                            sp.GetService<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>>())
                    },
                };
                return new PropertyTypeEvaluatorService(dictionary);
            });
            serviceCollection.AddScoped(c => mockQuoteRepository.Object);
            serviceCollection.AddSingleton<IClock>(c => SystemClock.Instance);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(
                c => c.Send(It.IsAny<GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(definition));

            serviceCollection.AddScoped(c => mockMediator.Object);
            return MockAutomationData.GetServiceProviderForEntityProviders(entityId, false, serviceCollection);
        }
    }
}
