// <copyright file="WebServiceTextProviderModelConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using Newtonsoft.Json;
    using UBind.Application.Export;
    using UBind.Application.Export.WebServiceTextProviders;
    using Xunit;

    /// <summary>
    /// Unit tests for WebServiceTextProviderModelConverter.
    /// </summary>
    public class WebServiceTextProviderModelConverterTests
    {
        [Fact]
        public void CreatesFixedTextProvider_WhenInputIsString()
        {
            // Arrange
            var json = @"{ ""text"": ""content-type:application/json""}";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyProviderModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as FixedTextWebServiceTextProviderModel);
        }

        [Fact]
        public void CreatesDotLiquidTemplateTextProviderModel_WhenInputHasTypeFormField()
        {
            // Arrange
            var json = @"{
    ""text"": {
        ""type"": ""dotLiquid"",
        ""templateString"": ""<p>Hello World!</p>"",
    }
}";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyProviderModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as DotLiquidTemplateTextProviderModel);
        }

        [Fact]
        public void CreatesUrlWebServiceTextProviderModel_WhenInputHasTypeEnvironment()
        {
            // Arrange
            var json = @"{
    ""text"": {
        ""type"": ""url"",
        ""urlString"": ""blah"",
        ""queryParameters"": [ ""boo:boo!"", ""jolly:bee""],
        ""pathParameter"": ""me""
    }
}";
            var converter = this.BuildConverter();

            // Act
            var model = JsonConvert.DeserializeObject<DummyProviderModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as UrlWebServiceTextProviderModel);
        }

        [Fact]
        public void CreatesFormFieldWebServiceTextProviderModel_WhenInputHasTypeFormField()
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
            var model = JsonConvert.DeserializeObject<DummyProviderModel>(json, converter);

            // Assert
            Assert.NotNull(model.Text as FormFieldWebServiceTextProviderModel);
        }

        private WebServiceTextProviderModelConverter BuildConverter()
        {
            return new WebServiceTextProviderModelConverter(
                new TypeMap
                {
                    { "fixed", typeof(FixedTextWebServiceTextProviderModel) },
                    { "dotLiquid", typeof(DotLiquidTemplateTextProviderModel) },
                    { "url", typeof(UrlWebServiceTextProviderModel) },
                    { "formField", typeof(FormFieldWebServiceTextProviderModel) },
                });
        }

        private class DummyProviderModel
        {
            public IExporterModel<IWebServiceTextProvider> Text { get; set; }
        }
    }
}
