// <copyright file="TextProviderModelConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using Xunit;

    public class TextProviderModelConverterTests
    {
        [Fact]
        public void CreatesFixedTextProviderModel_WhenInputIsString()
        {
            // Arrange
            var json = @"{ ""text"": ""boris@example.com"" }";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as FixedTextProviderModel);
        }

        [Fact]
        public void CreatesFormFieldTextProviderModel_WhenInputHasTypeFormField()
        {
            // Arrange
            var json = @"{
    ""text"": {
        ""type"": ""formField"",
        ""fieldName"": ""email"",
    }
}";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as FormFieldTextProviderModel);
        }

        [Fact]
        public void CreatesEnvironmentTextProviderModel_WhenInputHasTypeEnvironment()
        {
            // Arrange
            var json = @"{
    ""text"": {
        ""type"": ""environment"",
        ""default"": ""blah"",
    }
}";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as EnvironmentTextProviderModel);
        }

        private TextProviderModelConverter BuildConverter()
        {
            return new TextProviderModelConverter(
                new TypeMap
                {
                    { "fixed", typeof(FixedTextProviderModel) },
                    { "formField", typeof(FormFieldTextProviderModel) },
                    { "environment", typeof(EnvironmentTextProviderModel) },
                });
        }

        private class DummyModel
        {
            public IExporterModel<ITextProvider> Text { get; set; }
        }
    }
}
