// <copyright file="DecimalExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class DecimalExtensionsTests
    {
        [Fact]
        public void ToDollarsAndCents_RoundsToWholeNumberOfCents()
        {
            List<Tuple<double, string>> tupleList = new List<Tuple<double, string>>();
            tupleList.Add(new Tuple<double, string>(123.456, "$123.46"));
            tupleList.Add(new Tuple<double, string>(1234.567, "$1,234.57"));
            tupleList.Add(new Tuple<double, string>(1234567.89, "$1,234,567.89"));
            tupleList.Add(new Tuple<double, string>(0, "$0.00"));
            tupleList.Add(new Tuple<double, string>(-0, "$0.00"));
            tupleList.Add(new Tuple<double, string>(-123.456, "-$123.46"));
            tupleList.Add(new Tuple<double, string>(-1234.567, "-$1,234.57"));
            tupleList.Add(new Tuple<double, string>(-1234567.89, "-$1,234,567.89"));

            foreach (var tuple in tupleList)
            {
                // Arrange
                // DataRow does not support decimals, so we have to construct them from ints.
                var myDecimal = (decimal)tuple.Item1;

                // Act
                var output = myDecimal.ToDollarsAndCents();

                // Assert
                output.Should().Be(tuple.Item2);
            }
        }
    }
}
