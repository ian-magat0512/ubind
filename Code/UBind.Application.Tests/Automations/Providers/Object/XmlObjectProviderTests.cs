// <copyright file="XmlObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class XmlObjectProviderTests : IAsyncLifetime
    {
        private readonly Error xmlError = Errors.Automation.InvalidXmlTextValue("xmlTextToObject", null);
        private MockAutomationData automationData;

        public async Task InitializeAsync()
        {
            this.automationData = await MockAutomationData.CreateWithHttpTrigger();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task XmlObjectProvider_ShouldCreateDataObject_FromValidXmlValue()
        {
            // Arrange
            // var textTemplate = "Employee (age): {{ employee.name }} ({{ employee.age }})";
            var xmlString = @"<employee><name>Peter</name><age>25</age></employee>";
            var provider = this.BuildxmlObjectProvider(xmlString);

            // Act
            var data = (await provider.Resolve(new ProviderContext(this.automationData))).GetValueOrThrowIfFailed();

            // Assert
            var json = JsonConvert.SerializeObject(data.DataValue);
            var jObject = JObject.Parse(json);

            DataObjectHelper.CountProperties(data.DataValue).Should().Be(1);
            jObject.SelectToken("employee")["name"].ToString().Should().Be("Peter");
            jObject.SelectToken("employee")["age"].ToString().Should().Be("25");
        }

        [Fact]
        public async Task XmlObjectProvider_ShouldThrowErrorException_FromInvalidXmlValueAsync()
        {
            // Arrange
            // var automationData = MockAutomationData.CreateContextWithHttpTrigger();
            var xmlString = @"<Invalid Xml>";
            var provider = this.BuildxmlObjectProvider(xmlString);

            // Act
            Func<Task> func = async () => await provider.Resolve(new ProviderContext(this.automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Message.Should().Be(this.xmlError.Message);
        }

        [Fact]
        public async Task XmlObjectProvider_ShouldThrowErrorException_FromEmptyXmlValueAsync()
        {
            // Arrange
            var xmlString = string.Empty;
            var provider = this.BuildxmlObjectProvider(xmlString);

            // Act
            Func<Task> func = async () => await provider.Resolve(new ProviderContext(this.automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Message.Should().Be(this.xmlError.Message);
        }

        private IObjectProvider BuildxmlObjectProvider(string xmlString)
        {
            var textProviderModel = new StaticBuilder<Data<string>>() { Value = xmlString };
            var providerModel = new XmlTextToObjectProviderConfigModel() { TextProvider = textProviderModel };
            var objectProvider = providerModel.Build(null);
            return objectProvider;
        }
    }
}
