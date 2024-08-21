// <copyright file="DynamicObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class DynamicObjectProviderTests
    {
        [Theory]
        [InlineData("[{ \"propertyName\": \"combined\", \"value\": [ \"one\", 2, [ { \"propertyName\": \"three\", \"value\": 3 } ], [ 4, 5, 6 ] ]}]")]
        [InlineData("[{ \"propertyName\": \"static\", \"value\": 123.34 }]")]
        public async Task DynamicObjectProvider_ShouldBeAbleToParse_InlineObject(string inlineConfig)
        {
            // Arrange
            var providerModel = JsonConvert.DeserializeObject<IBuilder<IObjectProvider>>(inlineConfig, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var provider = providerModel.Build(MockAutomationData.GetDefaultServiceProvider());

            // Act
            var dictionaryObject = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var dataObject = ObjectHelper.ToDictionary(dictionaryObject.GetValueFromGeneric()) as Dictionary<string, object>;
            dataObject.Should().NotBeNull();
            dataObject.Keys.Should().HaveCount(1);
        }

        [Fact]
        public async Task DynamicObjectProvider_ShouldBeAbleToParse_InlineObjectWithProviderInside()
        {
            var inlineConfig = "[{ \"propertyName\": \"fromPath\", \"value\": [ \"hello\", \"from\", { \"objectPathLookupText\": \"/trigger/httpRequest/actionPath\" } ] }]";
            var providerModel = JsonConvert.DeserializeObject<IBuilder<IObjectProvider>>(inlineConfig, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var provider = providerModel.Build(MockAutomationData.GetDefaultServiceProvider());

            // Act
            var dictionaryObject = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var dataObject = ObjectHelper.ToDictionary(dictionaryObject.GetValueFromGeneric()) as Dictionary<string, object>;
            dataObject.TryGetValue("fromPath", out object values);

            var httpTriggerData = automationData.Trigger as HttpTriggerData;
            var fullString = string.Join(" ", values as List<object>);

            fullString.Should().Be($"hello from {httpTriggerData.HttpRequest.ActionPath}");
        }
    }
}
