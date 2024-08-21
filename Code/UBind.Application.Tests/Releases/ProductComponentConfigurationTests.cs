// <copyright file="ProductComponentConfigurationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Releases
{
    using FluentAssertions;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ProductComponentConfigurationTests
    {
        [Fact]
        public void FormDataSchema_IsCorrectlyDeserialized_FromConfiguraitonJson()
        {
            // Arrange
            var fakeRelease = FakeReleaseBuilder
                .CreateForProduct()
                .WithQuoteFormConfiguration(ConfigurationJsonFactory.GetSampleBaseConfigurationWithQuestionMetadata())
                .BuildRelease();

            var sut = new ProductComponentConfiguration(fakeRelease.QuoteDetails, new FieldSerializationBinder());

            // Act
            var formDataSchema = sut.FormDataSchema;

            // Assert
            formDataSchema.HasValue.Should().BeTrue();
            formDataSchema.Value.GetQuestionMetaData().Should().HaveCount(1);
        }

        [Fact]
        public void ProductConfiguration_CorrectlyIncludesFormDataSchema_WhenItIsDefinedInJson()
        {
            // Arrange
            var fakeRelease = FakeReleaseBuilder
                .CreateForProduct()
                .WithQuoteFormConfiguration(ConfigurationJsonFactory.GetSampleBaseConfigurationWithQuestionMetadata())
                .WithQuoteProductConfiguration(ProductConfigurationJsonFactory.CreateForQuoteApp())
                .BuildRelease();

            var sut = new ProductComponentConfiguration(fakeRelease.QuoteDetails, new FieldSerializationBinder());

            // Act
            var productConfiguration = sut.ProductConfiguration;

            // Assert
            productConfiguration.FormDataSchema.GetQuestionMetaData().Should().HaveCount(1);
        }
    }
}
