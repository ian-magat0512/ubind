// <copyright file="PropertyExpressionProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Expression
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class PropertyExpressionProviderTests
    {
        [Theory]
        [InlineData("simple.x", ".Simple.X")]
        [InlineData("compoundPropertyName.x", ".CompoundPropertyName.X")]
        public async Task Resolve_FindsCorrectProperty_WhenPathUsesCamelCase(string path, string expectedExpression)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(path);
            var sut = new PropertyExpressionProvider(pathProvider);

            // Act
            var result = await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            result.ToString().Should().EndWith(expectedExpression);
        }

        [Theory]
        [InlineData("Simple.x")]
        [InlineData("simple.X")]
        [InlineData("CompoundPropertyName.x")]
        [InlineData("compoundPropertyName.X")]
        public async Task Resolve_ThrowsErrorException_WhenPathUsesPascalCaseAsync(string pathWithPascalCaseSegment)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(pathWithPascalCaseSegment);
            var sut = new PropertyExpressionProvider(pathProvider);

            // Act
            Func<Task> func = async () => await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.syntax.error");
        }

        [Theory]
        [InlineData("wrong")]
        [InlineData("simple.y")]
        [InlineData("compoundpropertyname.y")]
        [InlineData("compoundPropertyname.y")]
        public async Task Resolve_ThrowsErrorException_WhenPathNotFoundAsync(string path)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(path);
            var sut = new PropertyExpressionProvider(pathProvider);

            // Act
            Func<Task> func = async () => await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.not.found");
        }

        private class Bar
        {
            public int X { get; set; }
        }

        private class Foo
        {
            public Bar Simple { get; set; }

            public Bar CompoundPropertyName { get; set; }
        }
    }
}
