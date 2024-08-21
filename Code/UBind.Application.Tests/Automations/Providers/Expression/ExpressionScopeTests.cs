// <copyright file="ExpressionScopeTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Expression
{
    using System;
    using FluentAssertions;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class ExpressionScopeTests
    {
        [Fact]
        public void PushWithGeneratedAlias_CreatesNumberAliases_WhenDuplicateRootsExist()
        {
            // Arrrange
            var parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(int));
            var sut = new ExpressionScope("foo", parameterExpression);
            var param1 = System.Linq.Expressions.Expression.Parameter(typeof(string));
            var param2 = System.Linq.Expressions.Expression.Parameter(typeof(bool));

            // Act
            sut.PushWithGeneratedAlias("foo", param1, "fakeProvider");
            sut.PushWithGeneratedAlias("foo", param2, "fakeProvider");

            // Assert
            sut.GetParameterExpression("foo1", "fakeProvider", null).Should().Be(param1);
            sut.GetParameterExpression("foo2", "fakeProvider", null).Should().Be(param2);
        }

        [Fact]
        public void Push_Throws_WhenAliasAlreadyExists()
        {
            // Arrrange
            var parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(int));
            var sut = new ExpressionScope("foo", parameterExpression);

            // Act
            Action action = () => sut.Push("foo", parameterExpression, "fakeProvider");

            // Assert
            action.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("automation.providers.duplicate.expression.alias");
        }
    }
}
