// <copyright file="ObjectContainsPropertyConditionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class ObjectContainsPropertyConditionTests
    {
        [Fact]
        public async Task IsPermitted_ResultsToTrue_WhenInitializedWithDynamicDataObject()
        {
            // Arrange
            var dataContext = await MockAutomationData.CreateWithHttpTrigger();
            List<ObjectPropertyConfigModel> properties = new List<ObjectPropertyConfigModel>();
            for (var a = 0; a < 5; a++)
            {
                properties.Add(new ObjectPropertyConfigModel
                {
                    PropertyName = new StaticBuilder<Data<string>> { Value = $"foo{a}" },
                    Value = (IBuilder<IProvider<IData>>)new StaticBuilder<Data<string>> { Value = $"bazz{10 - a}" },
                });
            }

            var propertyNameProvider = new StaticBuilder<Data<string>> { Value = "foo1" };
            var dataObjectProvider = new DynamicObjectProviderConfigModel { Properties = properties };
            var objectContainsPropertyCondition = new ObjectContainsPropertyConditionConfigModel
            {
                PropertyName = propertyNameProvider,
                Object = dataObjectProvider,
            };
            var condition = objectContainsPropertyCondition.Build(new Mock<IServiceProvider>().AddLoggers().Object);

            // Act
            var result = (await condition.Resolve(new ProviderContext(dataContext))).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ObjectContainsPropertyCondition_ResultToTrue_WhenInitializedWithPathObjectProvider()
        {
            // Arrange
            var dataContext = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var staticStringPath = new StaticBuilder<Data<string>> { Value = "/trigger/httpRequest/content" };
            var fixedPathLookupProviderModel = new FixedObjectPathLookupConfigModel() { Path = staticStringPath };
            var pathLookupTextProviderModel = new PathLookupTextProviderConfigModel { PathLookup = fixedPathLookupProviderModel };
            var jsonObjectProviderModel = new JsonTextToObjectProviderConfigModel { TextProvider = pathLookupTextProviderModel };

            var propertyNameProvider = new StaticBuilder<Data<string>> { Value = "isTrue" };
            var objectContainsPropertyConditionModel = new ObjectContainsPropertyConditionConfigModel { PropertyName = propertyNameProvider, Object = jsonObjectProviderModel };
            var condition = objectContainsPropertyConditionModel.Build(null);

            // Act + Assert
            var objectContains = (await condition.Resolve(new ProviderContext(dataContext))).GetValueOrThrowIfFailed();
            objectContains.DataValue.Should().BeTrue();
        }
    }
}
