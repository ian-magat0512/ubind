// <copyright file="DefaultPolicyLimitTimesPolicyTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using NodaTime;
    using UBind.Domain.Services;
    using Xunit;

    public class DefaultPolicyLimitTimesPolicyTests
    {
        [Fact]
        public void GetInceptionDate_CalculatedCorrectInstant_InAest()
        {
            // Arrange
            var inceptionDate = new LocalDate(2018, 10, 3);
            var sut = new DefaultPolicyTransactionTimeOfDayScheme();

            // Act
            var time = sut.GetInceptionTimestamp(inceptionDate);

            // Assert
            // 2018-10-03 16:00 AEST = 1018-10-03 06:00 UTC = 15385464000000000 ticks since epoch.
            Assert.Equal(15385464000000000, time.ToUnixTimeTicks());
        }
    }
}
