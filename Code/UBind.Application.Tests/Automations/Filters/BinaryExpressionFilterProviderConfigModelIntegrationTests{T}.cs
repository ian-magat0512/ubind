// <copyright file="BinaryExpressionFilterProviderConfigModelIntegrationTests{T}.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Tests.Automations.Fakes;

    public abstract class BinaryExpressionFilterProviderConfigModelIntegrationTests<T>
    {
        protected async Task VerifyConditionResolution(string json, T existingItem, bool expectedResult)
        {
            // Arrange
            var dependencyProvider = new Mock<IServiceProvider>();
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var sut = JsonConvert.DeserializeObject<IBuilder<IFilterProvider>>(json, AutomationDeserializationConfiguration.ModelSettings);
            var items = new List<T> { existingItem };
            var expectedCount = expectedResult ? 1 : 0;

            // Act
            var provider = sut!.Build(dependencyProvider.Object);

            // Assert
            var filter = await provider.Resolve<T>(
                new ProviderContext(automationData),
                new ExpressionScope("foo", Expression.Parameter(typeof(T))));
            var filteredItems = items.AsQueryable().Where(filter);
            filteredItems.Should().HaveCount(expectedCount);
        }
    }
}
