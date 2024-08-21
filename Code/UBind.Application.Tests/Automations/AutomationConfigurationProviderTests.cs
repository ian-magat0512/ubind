// <copyright file="AutomationConfigurationProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AutomationConfigurationProviderTests
    {
        private readonly DeploymentEnvironment environment = DeploymentEnvironment.Development;
        private readonly Mock<IAutomationConfigurationModelProvider> mockModelProvider;
        private readonly IServiceProvider mockDependencyProvider;
        private readonly Mock<ICachingResolver> mockCachingResolver;
        public AutomationConfigurationProviderTests()
        {
            this.mockModelProvider = new Mock<IAutomationConfigurationModelProvider>();
            this.mockDependencyProvider = MockAutomationData.GetDefaultServiceProvider();
            this.mockCachingResolver = new Mock<ICachingResolver>();
        }

        [Fact]
        public async Task Provider_ShouldReturnConfiguration_WhenAutomationParsedIsValid()
        {// Arrange
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""Test"",
                            ""alias"": ""testString"",
                            ""description"": ""test config for automation matching"",
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""Foo HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""Ash's pokemon is called {{name}}"",
                                                    ""dataObject"" : {
                                                        ""objectPathLookupObject"" : ""actions.fooAction.httpResponse.content""
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""Foo Http Action"",
                                        ""alias"": ""fooAction"",
                                        ""description"": ""running foo action"",
                                        ""asynchronous"": false,
                                        ""httpRequest"": {
                                            ""httpVerb"": ""GET"",
                                            ""url"": ""https://foo.com""
                                        }
                                    }
                                },
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""Foo Http Action"",
                                        ""alias"": ""foozAction"",
                                        ""description"": ""running fooz action"",
                                        ""asynchronous"": false,
                                        ""httpRequest"": {
                                            ""httpVerb"": ""GET"",
                                            ""url"": ""https://foo.com""
                                        }
                                    }
                                }
                            ]
                        }
                    ]
            }";

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            this.mockModelProvider.Setup(x => x.GetAutomationConfigurationOrNull(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.environment,
                null))
                .Returns(automationConfigurationModel);
            var provider = new AutomationConfigurationProvider(
                this.mockModelProvider.Object,
                this.mockDependencyProvider,
                this.mockCachingResolver.Object);

            // Act
            var automation = await provider.GetAutomationConfigurationOrNull(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                this.environment,
                null);

            // Assert
            var expectedAutomation = automationConfigurationModel.Build(this.mockDependencyProvider).ToString();
            automation.ToString().Should().Be(expectedAutomation);
        }
    }
}
