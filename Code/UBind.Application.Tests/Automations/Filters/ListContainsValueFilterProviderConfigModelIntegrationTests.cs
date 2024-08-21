// <copyright file="ListContainsValueFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ListContainsValueFilterProviderConfigModelIntegrationTests
    {
        [Fact]
        public async void Build_CreatesListContainsValueFilterAndResolvesToTrue_WhenBuiltFromValidJson()
        {
            await this.VerifyCondition("alpha", true);
        }

        [Fact]
        public async void Build_CreatesListContainsValueFilterAndResolvesToFalse_WhenBuiltFromValidJson()
        {
            await this.VerifyCondition("theta", false);
        }

        private async Task VerifyCondition(string value, bool expectedResult)
        {
            var json = $@"
            {{
                ""listContainsValueCondition"": {{
                    ""list"": {{
                        ""objectPathLookupList"": ""#""
                    }},
                    ""value"":  ""{value}""
                }}
            }}";
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var list = new List<object>()
            {
                new List<string> { "alpha", "beta", "gamma" },
                new List<string> { "delta", "epsilon", "zeta" },
            };
            var dataList = new GenericDataList<object>(list);

            // Act
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var provider = sut!.Build(dependencyProvider.Object);
            var filteredItems = await dataList.Where(null, provider, new ProviderContext(automationData));

            // Assert
            var count = expectedResult ? 1 : 0;
            filteredItems.Should().HaveCount(count);
        }
    }
}
