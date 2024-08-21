// <copyright file="AdditionalPropertyTransformHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyValue;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;
using Moq;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Queries.AdditionalPropertyValue;
using NodaTime;
using UBind.Domain.Dto;
using FluentAssertions;

public class AdditionalPropertyTransformHelperTests
{
    private readonly Mock<ICqrsMediator> mediatorMock = new Mock<ICqrsMediator>();
    private readonly Mock<IAdditionalPropertyDefinitionRepository> additionalPropertyDefinitionRepositoryMock = new Mock<IAdditionalPropertyDefinitionRepository>();

    [Fact]
    public async Task TransformObjectDictionaryToValueUpsertModels_ReturnsListWithSingleItem_WhenAdditionalPropertyHasValidType()
    {
        // Arrange
        var additionalPropertyAlias = "ValidProperty";
        var additionalPropertyValue = "ValidValue";
        var additionalPropertyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { additionalPropertyAlias, additionalPropertyValue },
        });
        var additionalPropertyDefinition = GetAdditionalPropertyDefinition(additionalPropertyAlias, additionalPropertyValue);

        this.additionalPropertyDefinitionRepositoryMock
            .Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
            .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel> { additionalPropertyDefinition });

        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act
        var result = await helper.TransformObjectDictionaryToValueUpsertModels(
            Guid.NewGuid(),
            null,
            AdditionalPropertyEntityType.None,
            additionalPropertyDictionary);

        // Assert
        result.Should().HaveCount(1);
        result[0].Value.Should().Be(additionalPropertyValue);
    }

    [Fact]
    public async Task TransformObjectDictionaryToValueUpsertModels_ThrowsError_WhenNonUniqueValueForUniqueAdditionalProperty()
    {
        // Arrange
        var additionalPropertyAlias = "UniqueProperty";
        var additionalPropertyValue = "NonUniqueValue";
        var additionalPropertyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { additionalPropertyAlias, additionalPropertyValue },
        });
        var additionalPropertyDefinition = GetAdditionalPropertyDefinition(additionalPropertyAlias, additionalPropertyValue, isUnique: true);
        var existingValues = new List<AdditionalPropertyValueDto>
        {
            new AdditionalPropertyValueDto(),
        };

        this.additionalPropertyDefinitionRepositoryMock
            .Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
            .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel> { additionalPropertyDefinition });

        this.mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAdditionalPropertyValuesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(existingValues));

        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ErrorException>(() =>
            helper.TransformObjectDictionaryToValueUpsertModels(
                Guid.NewGuid(),
                null,
                AdditionalPropertyEntityType.None,
                additionalPropertyDictionary));
        ex.Error.Code.Should().Be(
            Errors.AdditionalProperties.UniqueAdditionalPropertyValueAlreadyUsed(
                AdditionalPropertyEntityType.Quote, additionalPropertyAlias, additionalPropertyValue).Code);
    }

    [Fact]
    public async Task TransformObjectDictionaryToValueUpsertModels_ReturnsEmptyList_WhenAdditionalPropertyDictionaryIsNull()
    {
        // Arrange
        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act
        var result = await helper.TransformObjectDictionaryToValueUpsertModels(
            Guid.NewGuid(),
            null,
            AdditionalPropertyEntityType.Quote,
            null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task TransformObjectDictionaryToValueUpsertModels_ThrowsError_WhenRequiredAdditionalPropertyIsEmpty()
    {
        // Arrange
        var additionalPropertyAlias = "RequiredProperty";
        var additionalPropertyValue = string.Empty;

        var additionalPropertyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { additionalPropertyAlias, additionalPropertyValue },
        });

        var additionalPropertyDefinition = GetAdditionalPropertyDefinition(additionalPropertyAlias, additionalPropertyValue, isRequired: true);
        this.additionalPropertyDefinitionRepositoryMock
            .Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
            .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel> { additionalPropertyDefinition });

        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ErrorException>(() =>
            helper.TransformObjectDictionaryToValueUpsertModels(
                Guid.NewGuid(), null, AdditionalPropertyEntityType.None, additionalPropertyDictionary));
        ex.Error.Code.Should().Be(Errors.AdditionalProperties.RequiredAdditionalPropertyIsEmpty(additionalPropertyAlias).Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(123)]
    [InlineData(3.14F)]
    [InlineData('L')]
    public async Task TransformObjectDictionaryToValueUpsertModels_ThrowsError_WhenAdditionalPropertyHasInvalidType(object propertyValue)
    {
        // Arrange
        var additionalPropertyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { "PropertyKey", propertyValue },
        });

        var additionalPropertyDefinition = GetAdditionalPropertyDefinition("PropertyKey", "PropertyName", isRequired: true);
        this.additionalPropertyDefinitionRepositoryMock
            .Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
            .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel> { additionalPropertyDefinition });
        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ErrorException>(() =>
            helper.TransformObjectDictionaryToValueUpsertModels(
                Guid.NewGuid(),
                null,
                AdditionalPropertyEntityType.None,
                additionalPropertyDictionary));
        ex.Error.Code.Should().Be(Errors.AdditionalProperties.AdditionalPropertyHasInvalidType("alias", propertyValue.GetType()).Code);
    }

    [Fact]
    public async Task TransformObjectDictionaryToValueUpsertModels_ThrowsError_WhenAdditionalPropertyNotFound()
    {
        // Arrange
        var additionalPropertyDictionary = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            { "NonExistentProperty", "SomeValue" },
        });

        var additionalPropertyDefinition = GetAdditionalPropertyDefinition("ExistingProperty", "ExistingValue");
        this.additionalPropertyDefinitionRepositoryMock
            .Setup(x => x.GetByModelFilter(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyDefinitionReadModelFilters>()))
            .ReturnsAsync(new List<AdditionalPropertyDefinitionReadModel> { additionalPropertyDefinition });
        var helper = new AdditionalPropertyTransformHelper(this.additionalPropertyDefinitionRepositoryMock.Object, this.mediatorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ErrorException>(() =>
            helper.TransformObjectDictionaryToValueUpsertModels(
                Guid.NewGuid(),
                null,
                AdditionalPropertyEntityType.Quote,
                additionalPropertyDictionary));
        ex.Error.Code.Should().Be(Errors.AdditionalProperties.AdditionalPropertyNotFound(AdditionalPropertyEntityType.Quote, "NonExistentProperty").Code);
    }

    private static AdditionalPropertyDefinitionReadModel GetAdditionalPropertyDefinition(
        string additionalPropertyAlias, string additionalPropertyName, bool isRequired = false, bool isUnique = false)
    {
        return new AdditionalPropertyDefinitionReadModel(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Instant.MinValue,
            additionalPropertyAlias,
            additionalPropertyName,
            AdditionalPropertyEntityType.Quote,
            AdditionalPropertyDefinitionContextType.Tenant,
            Guid.NewGuid(),
            isRequired,
            isUnique,
            false,
            string.Empty,
            AdditionalPropertyDefinitionType.Text,
            AdditionalPropertyDefinitionSchemaType.None);
    }
}
