// <copyright file="EnvironmentTextProviderModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using Xunit;

    public class EnvironmentTextProviderModelTests
    {
        [Fact]
        public void Build_Succeeds_WhenAllProvidersAreSpecified()
        {
            // Arrange
            var json = @"
{
    ""text"":  {
        ""type"": ""environment"",
        ""default"": ""alpha"",
        ""development"": ""beta"",
        ""staging"": ""gamma"",
        ""production"": ""delta""
    }
}";
            var converter = this.BuildConverter();
            var sut = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Act
            var provider = sut.Text.Build(null, null);

            // Assert
            Assert.NotNull(provider as EnvironmentTextProvider);
        }

        [Fact]
        public void Build_Succeeds_WhenOnlyDefaultProviderIsSpecified()
        {
            // Arrange
            var json = @"
{
    ""text"":  {
        ""type"": ""environment"",
        ""default"": ""alpha""
    }
}";
            var converter = this.BuildConverter();
            var sut = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Act
            var provider = sut.Text.Build(null, null);

            // Assert
            Assert.NotNull(provider as EnvironmentTextProvider);
        }

        [Fact]
        public void Build_Throws_WhenDefaultProviderIsNotSpecifiedAnEnvironmentSpecificProviderIsMissing()
        {
            // Arrange
            var json = @"
{
    ""text"":  {
        ""type"": ""environment"",
        ""development"": ""beta"",
        ""staging"": ""gamma""
    }
}";
            var converter = this.BuildConverter();
            var sut = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Act
            Action act = () => sut.Text.Build(null, null);

            // Assert
            act.Should().Throw<IntegrationConfigurationException>();
        }

        private TextProviderModelConverter BuildConverter()
        {
            return new TextProviderModelConverter(
                new TypeMap
                {
                    { "environment", typeof(EnvironmentTextProviderModel) },
                });
        }

        private class DummyModel
        {
            public IExporterModel<ITextProvider> Text { get; set; }
        }
    }
}
