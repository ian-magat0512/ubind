// <copyright file="EventTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Entities
{
    using FluentAssertions;
    using UBind.Domain.Automation;
    using Xunit;

    public class EventTests
    {
        // Unix timestamp representing:
        // Mon Feb 15 2021 23:48:07.123 UTC
        // Tue Feb 16 2021 10:48:07.123 GMT+1100 (Australian Eastern Daylight Time)
        private const long TestTicksSinceEpoch = 16134328871230000;

        [Fact]
        public void CreatedDate_ReturnsCorrectDateInAet_WhenExpiryDateTimeIsSet()
        {
            // Arrange
            var sut = new Event
            {
                CreatedTicksSinceEpoch = TestTicksSinceEpoch,
            };

            // Act + Assert
            sut.CreatedDate.Should().Be("16 Feb 2021");
        }

        [Fact]
        public void CreatedTime_ReturnsCorrectTimeInAet_WhenExpiryDateTimeIsSet()
        {
            // Arrange
            var sut = new Event
            {
                CreatedTicksSinceEpoch = TestTicksSinceEpoch,
            };

            // Act + Assert
            sut.CreatedTime.Should().Be("10:48 am");
        }
    }
}
