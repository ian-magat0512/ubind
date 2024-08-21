// <copyright file="WebFromValidatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.Helpers;
    using Xunit;

    public class WebFromValidatorTests
    {
        [Theory]
        [ClassData(typeof(QuoteValidatorTestData))]
        public void QuoteValidator_ShouldVerifyQuoteTransactionalRequests_AndThrowException(
            Guid quoteId,
            Guid quoteTenant,
            Guid quoteProduct,
            Guid requestTenant,
            Guid requestProduct,
            DeploymentEnvironment quoteEnvironment,
            DeploymentEnvironment requestEnvironment,
            Type exceptionType,
            string errorCode)
        {
            // Arrange
            QuoteAggregate quoteAggregate = null;
            if (quoteId != default)
            {
                var quoteWorkflowProvider = new DefaultQuoteWorkflowProvider();
                var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
                var quote = QuoteAggregate.CreateNewBusinessQuote(
                    quoteTenant,
                    Guid.NewGuid(),
                    quoteProduct,
                    quoteEnvironment,
                    new QuoteExpirySettings(30),
                    default,
                    default,
                    Guid.NewGuid(),
                    Timezones.AET,
                    false,
                    null,
                    true);
                quoteAggregate = quote.Aggregate;
            }

            var productContext = new ProductContext(requestTenant, requestProduct, requestEnvironment);

            // Act
            var exception = Record.Exception(
                   () => WebFormValidator.ValidateQuoteRequest(
                       quoteId, quoteAggregate, productContext));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType(exceptionType);

            var error = exception as ErrorException;
            error.Error.Code.Should().Be(errorCode);
        }

        public class QuoteValidatorTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var id = Guid.NewGuid();
                yield return new object[] { default(Guid), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.None, DeploymentEnvironment.None, typeof(NotFoundException), "record.not.found" };
                yield return new object[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development, DeploymentEnvironment.Development, typeof(ErrorException), "action.forbidden" };
                yield return new object[] { Guid.NewGuid(), id, id, id, id, DeploymentEnvironment.Development, DeploymentEnvironment.Staging, typeof(ErrorException), "environment.mismatch" };
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
