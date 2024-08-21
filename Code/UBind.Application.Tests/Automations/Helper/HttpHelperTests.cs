// <copyright file="HttpHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Helper
{
    using System;
    using FluentAssertions;
    using UBind.Application.Automation.Helper;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class HttpHelperTests
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("UPDATEREDIRECTREF")]
        [InlineData("VERSION-CONTROL")]
        public void ThrowIfHttpVerbInvalid_DoesNotThrow_WhenHttpVerbInvalid(string httpVerb)
        {
            // Arrange

            // Act
            Action act = () => HttpHelper.ThrowIfHttpVerbInvalid(httpVerb);

            // Assert
            act.Should().NotThrow<ErrorException>();
        }

        [Theory]
        [InlineData("G")]
        [InlineData("_VERSION")]
        [InlineData("-VERSION")]
        [InlineData("VERSION-")]
        [InlineData("VERSION+CONTROL")]
        public void ThrowIfHttpVerbInvalid_Throw_WhenHttpVerbIsValid(string httpVerb)
        {
            // Arrange

            // Act
            Action act = () => HttpHelper.ThrowIfHttpVerbInvalid(httpVerb);

            // Assert
            act.Should().Throw<ErrorException>();
        }
    }
}
