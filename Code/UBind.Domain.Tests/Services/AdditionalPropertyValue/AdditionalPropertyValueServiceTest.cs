// <copyright file="AdditionalPropertyValueServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdditionalPropertyValueServiceTest : IClassFixture<AdditionalPropertyServiceTestFixture>
    {
        private readonly AdditionalPropertyServiceTestFixture fixture;

        public AdditionalPropertyValueServiceTest(AdditionalPropertyServiceTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task UpdatePropertiesOfEntityAggregate_ShouldBeOk_WhenEntityIsQuote()
        {
            // Assign
            var models = this.GetAdditionalPropertyValues();
            var quoteAggregate = this.CreateMockQuoteAggregateAndSetup();
            var quote = quoteAggregate.GetLatestQuote();
            var id = quote.Id;
            var clock = new TestClock();
            this.fixture.QuoteReadModelRepository
                .Setup(q => q.GetQuoteAggregateId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.fixture.HttpContextPropertiesResolver.Setup(pup => pup.PerformingUserId).Returns(Guid.NewGuid());
            this.fixture.ClockMock.Setup(cm => cm.GetCurrentInstant()).Returns(clock.GetCurrentInstant());
            this.fixture.QuoteAggregateRepositoryMock
                .Setup(qarm => qarm.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.fixture.QuoteAggregateRepositoryMock.Setup(qarm => qarm.Save(It.IsAny<QuoteAggregate>())).Returns(
                Task.FromResult(quoteAggregate));
            this.SetupTextReadWriteModelMock(models, id);
            this.fixture.AdditionalPropertyTransformHelperMock
                .Setup(a => a.GetAdditionalPropertyDefinitions(
                    It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(Task.FromResult(new List<AdditionalPropertyDefinitionReadModel>()));

            // Act
            await this.fixture.AdditionalPropertyValueService.UpdatePropertiesOfEntityAggregate(
                quoteAggregate.TenantId,
                AdditionalPropertyEntityType.Quote,
                models,
                id,
                DeploymentEnvironment.Development);

            // Assert
            // this.fixture.QuoteReadModelRepository.VerifyAll();
            this.fixture.QuoteAggregateRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task UpdatePropertiesOfEntityAggregate_ShouldBeOk_WhenEntityIsQuoteVersion()
        {
            // Assign
            var models = this.GetAdditionalPropertyValues();
            var id = Guid.NewGuid();
            var quoteAggregate = this.CreateMockQuoteAggregateAndSetup();
            var quote = quoteAggregate.GetLatestQuote();
            var tenantId = Guid.NewGuid();
            var clock = new TestClock();
            var quoteVersionReadModelDetails = new Mock<IQuoteVersionReadModelWithRelatedEntities>();
            var quoteVersionReadModel = new QuoteVersionReadModel
            {
                AggregateId = quoteAggregate.Id,
                QuoteVersionNumber = 1,
                QuoteId = quote.Id,
            };

            this.fixture.QuoteVersionReadModelRepositoryMock
                .Setup(q => q.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), DeploymentEnvironment.Development, id, It.IsAny<IEnumerable<string>>()))
                .Returns(quoteVersionReadModelDetails.Object);
            quoteVersionReadModelDetails.SetupGet(q => q.QuoteVersion).Returns(quoteVersionReadModel);
            this.fixture.HttpContextPropertiesResolver.Setup(pup => pup.PerformingUserId).Returns(Guid.NewGuid());
            this.fixture.ClockMock.Setup(cm => cm.GetCurrentInstant()).Returns(clock.GetCurrentInstant());
            this.fixture.QuoteAggregateRepositoryMock
                .Setup(qarm => qarm.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.fixture.QuoteAggregateRepositoryMock.Setup(qarm => qarm.Save(It.IsAny<QuoteAggregate>())).Returns(
                Task.FromResult(quoteAggregate));
            this.SetupTextReadWriteModelMock(models, id);
            this.fixture.AdditionalPropertyTransformHelperMock
                .Setup(a => a.GetAdditionalPropertyDefinitions(
                    It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(Task.FromResult(new List<AdditionalPropertyDefinitionReadModel>()));

            // Act
            await this.fixture.AdditionalPropertyValueService.UpdatePropertiesOfEntityAggregate(
                tenantId,
                AdditionalPropertyEntityType.QuoteVersion,
                models,
                id,
                DeploymentEnvironment.Development);

            // Assert
            this.fixture.QuoteVersionReadModelRepositoryMock.VerifyAll();
            this.fixture.QuoteAggregateRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task UpdatePropertiesOfEntityAggregate_ValidProperties_ShouldSucceed()
        {
            // Arrange
            var quoteAggregate = this.CreateMockQuoteAggregateAndSetup();
            var quote = quoteAggregate.GetLatestQuote();
            var id = quote.Id;
            var tenantId = quoteAggregate.TenantId;
            var entityType = AdditionalPropertyEntityType.Quote;
            var entityId = quote.Id; // Guid.NewGuid();
            var definitionId = Guid.NewGuid();
            var models = new List<AdditionalPropertyValueUpsertModel>
            {
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = definitionId,
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "validValue",
                },
            };

            this.SetupTextReadWriteModelMock(models, quoteAggregate.Id);
            this.fixture.ClockMock.Setup(cm => cm.GetCurrentInstant()).Returns(new TestClock().GetCurrentInstant());
            this.fixture.HttpContextPropertiesResolver.Setup(p => p.PerformingUserId).Returns(Guid.NewGuid());
            this.fixture.AdditionalPropertyTransformHelperMock
                .Setup(a => a.GetAdditionalPropertyDefinitions(
                    It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(Task.FromResult(new List<AdditionalPropertyDefinitionReadModel>()));

            this.fixture.QuoteReadModelRepository
                .Setup(q => q.GetQuoteAggregateId(It.IsAny<Guid>()))
                .Returns(quoteAggregate.Id);
            this.fixture.QuoteAggregateRepositoryMock
                .Setup(qarm => qarm.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            this.fixture.QuoteAggregateRepositoryMock
                .Setup(q => q.Save(It.IsAny<QuoteAggregate>()))
                .Returns(Task.FromResult(quoteAggregate));
            this.fixture.QuoteAggregateRepositoryMock
                .Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);

            this.fixture.AdditionalPropertyTransformHelperMock
                .Setup(a => a.GetAdditionalPropertyDefinitions(quoteAggregate.TenantId, null, entityType))
                .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel>
                {
                    new AdditionalPropertyDefinitionReadModel(
                        tenantId,
                        definitionId,
                        Instant.MinValue,
                        "test",
                        "test",
                        AdditionalPropertyEntityType.Quote,
                        AdditionalPropertyDefinitionContextType.Tenant,
                        tenantId,
                        true,
                        false,
                        false,
                        string.Empty,
                        AdditionalPropertyDefinitionType.Text,
                        AdditionalPropertyDefinitionSchemaType.None,
                        tenantId),
                });
            this.fixture.TextAdditionalPropertyValueRepositoryMock
                .Setup(t => t.GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                    tenantId, entityId, models[0].DefinitionId))
                .ReturnsAsync(new AdditionalPropertyValueDto
                {
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = models[0].DefinitionId,
                    },
                    Value = "test",
                });

            // Act
            var quoteNumber = await this.fixture.AdditionalPropertyValueService.UpdatePropertiesOfEntityAggregate(
                tenantId, entityType, models, entityId, DeploymentEnvironment.Development);

            // Assert
            quoteNumber.Should().Be(quoteAggregate.GetLatestQuote().QuoteNumber);
        }

        [Fact]
        public async Task UpdatePropertiesOfEntityAggregate_NonUniqueValueForUniqueProperty_ShouldThrowError()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var entityType = AdditionalPropertyEntityType.Quote;
            var entityId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;

            var models = new List<AdditionalPropertyValueUpsertModel>
            {
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = Guid.NewGuid(),
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "value one",
                },
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = Guid.NewGuid(),
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "value two",
                },
            };

            var definitions = new List<AdditionalPropertyDefinitionReadModel>
            {
                new AdditionalPropertyDefinitionReadModel(
                    tenantId,
                    models[0].DefinitionId,
                    Instant.MinValue,
                    "test",
                    "test",
                    entityType,
                    AdditionalPropertyDefinitionContextType.Tenant,
                    tenantId,
                    false,
                    true, // Set property as unique
                    false,
                    string.Empty,
                    AdditionalPropertyDefinitionType.Text,
                    AdditionalPropertyDefinitionSchemaType.None,
                    tenantId),
            };
            this.fixture.AdditionalPropertyTransformHelperMock
                .Setup(a => a.GetAdditionalPropertyDefinitions(tenantId, null, entityType))
                .ReturnsAsync(definitions);

            this.fixture.QuoteReadModelRepository
                .Setup(q => q.GetQuoteAggregateId(entityId))
                .Returns(Guid.NewGuid()); // Mock Quote Aggregate Id

            this.fixture.TextAdditionalPropertyValueRepositoryMock
                .Setup(t => t.GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                    tenantId, entityId, models[0].DefinitionId))
                .ReturnsAsync(new AdditionalPropertyValueDto
                {
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = models[0].DefinitionId,
                    },
                    Value = "existing value", // Mock an existing value
                });

            // Mock the query to simulate a non-unique value scenario.
            this.fixture.Mediator
                .Setup(m => m.Send(It.IsAny<IsAdditionalPropertyValueUniqueQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            // Act
            var ex = await Assert.ThrowsAsync<ErrorException>(() => this.fixture.AdditionalPropertyValueService.UpdatePropertiesOfEntityAggregate(
                tenantId, entityType, models, entityId, environment));

            // Assert
            ex.Error.Code.Should().Be(
                Errors.AdditionalProperties.UniqueAdditionalPropertyValueAlreadyUsed(entityType, "alias", "value").Code);
        }

        private QuoteAggregate CreateMockQuoteAggregateAndSetup()
        {
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quoteExpirySettingsProvider = new Mock<IQuoteExpirySettingsProvider>();
            var clock = new TestClock();
            var quoteExpirySettings = new Mock<IQuoteExpirySettings>();
            quoteExpirySettingsProvider.Setup(q => q.Retrieve(It.IsAny<QuoteAggregate>())).ReturnsAsync(
                quoteExpirySettings.Object);

            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenantId,
                Guid.NewGuid(),
                productId,
                DeploymentEnvironment.Development,
                quoteExpirySettings.Object,
                Guid.NewGuid(),
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET,
                quoteNumber: "Q-0322");
            return quote.Aggregate;
        }

        private void SetupTextReadWriteModelMock(
            List<AdditionalPropertyValueUpsertModel> models, Guid entityId)
        {
            for (var i = 0; i < models.Count; i++)
            {
                // just to cover both condition in if clause of the service.
                AdditionalPropertyValueDto dto = null;
                if (i != 0)
                {
                    dto = new AdditionalPropertyValueDto
                    {
                        AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                        {
                            Id = models[i].DefinitionId,
                        },
                        Value = models[i].Value,
                    };
                }

                this.fixture.TextAdditionalPropertyValueRepositoryMock.Setup(
                    t => t.GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                        It.IsAny<Guid>(), entityId, models[i].DefinitionId)).Returns(Task.FromResult(dto));
            }
        }

        private List<AdditionalPropertyValueUpsertModel> GetAdditionalPropertyValues()
        {
            var models = new List<AdditionalPropertyValueUpsertModel>
            {
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = Guid.NewGuid(),
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "value one",
                },
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = Guid.NewGuid(),
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "value two",
                },
                new AdditionalPropertyValueUpsertModel
                {
                    DefinitionId = Guid.NewGuid(),
                    Type = AdditionalPropertyDefinitionType.Text,
                    Value = "value three",
                },
            };
            return models;
        }
    }
}
