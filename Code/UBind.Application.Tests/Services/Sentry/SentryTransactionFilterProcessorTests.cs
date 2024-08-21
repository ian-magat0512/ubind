// <copyright file="SentryTransactionFilterProcessorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services.Sentry
{
    using FluentAssertions;
    using global::Sentry;
    using UBind.Application.Exceptions;
    using Xunit;

    public class SentryTransactionFilterProcessorTests
    {
        [Theory]
        [InlineData("api/v1/health/some-text")]
        [InlineData("api/v3/health/some-text1")]
        [InlineData("api/v1/health/some-text1")]
        [InlineData("api/v10/health/some-text1")]
        public void Test_Sentry_Health_Check_Url_Exclusion_Pattern_Is_Match(string param)
        {
            var transaction = new Transaction("validation test operation", "validation test operation")
            {
                Request = new Request()
                {
                    Url = param,
                },
            };
            new SentryTransactionFilterProcessor().Process(transaction).Should().BeNull();
        }

        [Theory]
        [InlineData("some/v1/health/some-text")]
        [InlineData("api/v3beta/health/some-text1")]
        [InlineData("portal/v1/health/some-text1")]
        [InlineData("portal/health/some-text1")]
        [InlineData("api/v10.02/health/some-text1")]
        [InlineData("api/v10-02/health/some-text1")]
        public void Test_Sentry_Health_Check_Url_Exclusion_Pattern_Should_Not_Match(string param)
        {
            var transaction = new Transaction("validation test operation", "validation test operation")
            {
                Request = new Request()
                {
                    Url = param,
                },
            };
            new SentryTransactionFilterProcessor().Process(transaction).Should().NotBeNull();
        }
    }
}
