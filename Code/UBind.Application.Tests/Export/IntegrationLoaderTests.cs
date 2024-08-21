// <copyright file="IntegrationLoaderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using System;
    using System.Linq;
    using Moq;
    using UBind.Application.Export;
    using UBind.Domain.Configuration;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class IntegrationLoaderTests
    {
        [Fact]
        public void Constructor_LoadsEmailEventExporterCorrectly()
        {
            // Arrange
            var dependencyProvider = new Mock<IExporterDependencyProvider>();
            var json = @"{
    ""eventExporters"" : [
        {
            ""id"" : ""MyTestExporter"",
            ""events"" : [ ""FormUpdated"", ""Calculated"" ],
            ""action"" : {
                ""type"" : ""email"",
                ""from"" : ""from@example.com"",
                ""to"" : ""to@example.com"",
                ""cc"" : ""cc@example.com"",
                ""bcc"" : ""bcc@example.com"",
                ""subject"" : ""Test message"",
                ""plainTextBody"" : ""Hello World"",
                ""htmlBody"" : ""<span>Hello World</span>""
            }
        }
    ]
}";

            // Act
            var model = IntegrationConfigurationParser.Parse(json);
            var config = model.Build(dependencyProvider.Object);

            // Assert
            Assert.True(config.GetExportersForEvent(Domain.ApplicationEventType.FormUpdated).Any());
        }

        [Fact]
        public void Constructor_LoadsWebIntegrationModelsCorrectly()
        {
            // Arrange
            var dependencyProvider = new Mock<IExporterDependencyProvider>();
            var productConfiguration = new Mock<IProductConfiguration>();
            var defaultWorkflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            var json = @"{
    ""webServiceIntegrations"" : [
        {
            ""id"" : ""MyTestIntegration"",
            ""requestType"": ""GET"",
            ""url"" : {
                ""type"": ""url"",
                ""urlString"": ""www.test.com"",
                ""pathParameter"": ""authId""
            },
            ""authMethod"": {
                ""authenticationType"": ""Bearer"",
                ""authToken"": ""1234456""
            },
            ""headers"": [""content-type:application/json"", ""x-correlation-ID:000"" ],
            ""responseTemplate"" : {
                ""type"": ""dotLiquid"",
                ""templateString"": ""<p>Hello World!</p>""
            }
        }  
    ]
}";

            // Act
            var model = IntegrationConfigurationParser.Parse(json);
            var config = model.Build(dependencyProvider.Object, productConfiguration.Object);

            // Assert
            Assert.Contains("MyTestIntegration", config.GetWebIntegrations(), StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
