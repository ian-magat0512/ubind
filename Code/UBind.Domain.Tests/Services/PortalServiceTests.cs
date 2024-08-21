// <copyright file="PortalServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using FluentAssertions;
    using UBind.Domain.Services;
    using Xunit;

    public class PortalServiceTests
    {
        [Theory]
        [InlineData("https://app.ubind.com.au/portal/test", "login", "https://app.ubind.com.au/portal/test/path/login")]
        [InlineData(
            "https://app.ubind.com.au/portal/test",
            "activate/d7613345-4e25-4256-803b-17ed1523cf8b?invitationId=d0d14d9c-6ac4-420b-b2b5-0d87c472898e",
            "https://app.ubind.com.au/portal/test/path/activate/d7613345-4e25-4256-803b-17ed1523cf8b?invitationId=d0d14d9c-6ac4-420b-b2b5-0d87c472898e")]
        [InlineData(
            "https://app.ubind.com.au/portal/test?environment=Development",
            "activate/d7613345-4e25-4256-803b-17ed1523cf8b?invitationId=d0d14d9c-6ac4-420b-b2b5-0d87c472898e",
            "https://app.ubind.com.au/portal/test/path/activate/d7613345-4e25-4256-803b-17ed1523cf8b?environment=Development&invitationId=d0d14d9c-6ac4-420b-b2b5-0d87c472898e")]
        public void AddPathToDefaultUrl_AddsPath_RespectingQueryParameters(string defaultUrl, string path, string expectedResult)
        {
            // Arrange
            var portalService = new PortalService(null, null, null);

            // Act
            string result = portalService.AddPathToDefaultUrl(defaultUrl, path);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
