// <copyright file="PropertyTypeEvaluatorServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using Xunit;

    public class PropertyTypeEvaluatorServiceTest
    {
        private readonly Mock<ITextAdditionalPropertyValueReadModelRepository> textAdditionalPropertyValueRepositoryMock;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;
        private readonly Mock<IClock> clockService;
        private readonly Mock<ITextAdditionalPropertyValueAggregateRepository> textAdditionalPropertyValueAggregateRepositoryMock;
        private readonly Mock<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>> textAdditionalPropertyWritableReadModelMock;

        public PropertyTypeEvaluatorServiceTest()
        {
            this.clockService = new Mock<IClock>(MockBehavior.Strict);
            this.textAdditionalPropertyValueRepositoryMock = new Mock<ITextAdditionalPropertyValueReadModelRepository>(
                MockBehavior.Strict);
            this.textAdditionalPropertyValueAggregateRepositoryMock =
                new Mock<ITextAdditionalPropertyValueAggregateRepository>(MockBehavior.Strict);
            this.textAdditionalPropertyWritableReadModelMock =
                new Mock<IWritableReadModelRepository<TextAdditionalPropertyValueReadModel>>(MockBehavior.Strict);
            var dictionary = new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>
            {
                {
                    AdditionalPropertyDefinitionType.Text,
                    new TextAdditionalPropertyValueProcessor(
                        this.textAdditionalPropertyValueRepositoryMock.Object,
                        this.clockService.Object,
                        this.textAdditionalPropertyValueAggregateRepositoryMock.Object,
                        this.textAdditionalPropertyWritableReadModelMock.Object)
                },
            };

            this.propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(dictionary);
        }

        [Theory]
        [InlineData(AdditionalPropertyDefinitionType.Text)]
        public void GeneratePropertyTypeEvaluator_ShouldBeOk_WhenPropertyTypeIsValid(
            AdditionalPropertyDefinitionType propertyType)
        {
            // Act
            var propertyEvaluator = this.propertyTypeEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
                propertyType);

            // Assert
            propertyEvaluator.Should().NotBeNull();
        }
    }
}
