// <copyright file="ObjectContainsPropertyFilterProviderConfigModelIntegrationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using System.Linq.Expressions;
    using System.Reflection;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ObjectContainsPropertyFilterProviderConfigModelIntegrationTests
    {
        [Fact]
        public async Task Build_ShouldCreateFilter_AndResolveToTrueIfPropertyExists()
        {
            await this.VerifyCondition("name", true);
        }

        [Fact]
        public async Task Build_ShouldCreateFilter_AndResolveValueToFalseIfPropertyDoesNotExist()
        {
            await this.VerifyCondition("occupation", false);
        }

        /// <summary>
        /// Remark: This unit test assumes schema validation did not work.
        /// </summary>
        /// <returns>A task.</returns>
        [Fact]
        public async Task Build_ShouldCreateFilter_AndThrowAnExceptionIfOutputOfPathLookupIsNotAnObject()
        {
            var json = $@"
            {{
                ""objectContainsPropertyCondition"": {{
                    ""object"": {{
                        ""objectPathLookupObject"": ""#""
                    }},
                    ""propertyName"": ""name""
                }}
            }}";
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var items = new List<object>()
            {
                new Dictionary<string, object>() { { "name", "Foo"}, { "age", 20 }, },
                new Dictionary<string, object>() { { "age", 22 }, { "address", "Bar st" }, },
            };

            // Act
            var filterProvider = sut!.Build(dependencyProvider.Object);
            var result = await filterProvider.Resolve(new ProviderContext(automationData), new ExpressionScope("item", Expression.Parameter(typeof(object))));
            var lambdaExpression = (LambdaExpression)result;

            // Assert
            Action act = () => lambdaExpression.Compile().DynamicInvoke("testString");
            act.Should().Throw<TargetInvocationException>()
                .WithInnerException<ErrorException>();
        }

        private async Task VerifyCondition(string propertyToFind, bool expectedResult)
        {
            var json = $@"
            {{
                ""objectContainsPropertyCondition"": {{
                    ""object"": {{
                        ""objectPathLookupObject"": ""#""
                    }},
                    ""propertyName"": ""{propertyToFind}""
                }}
            }}";
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var items = new List<object>()
            {
                new Dictionary<string, object>() { { "name", "Foo"}, { "age", 20 }, },
                new Dictionary<string, object>() { { "age", 22 }, { "address", "Bar st" }, },
            };
            var genericDataList = new GenericDataList<object>(items);
            var count = expectedResult ? 1 : 0;

            // Act
            var filterProvider = sut!.Build(dependencyProvider.Object);

            // Assert
            var filteredItems = await genericDataList.Where(null, filterProvider, new ProviderContext(automationData));
            filteredItems.Should().HaveCount(count);
        }
    }
}
