// <copyright file="DeftAccessTokenProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Deft
{
    using System.Net;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Payment.Deft;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DeftAccessTokenProviderTests
    {
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetAccessToken_ReturnsAccessToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var clock = SystemClock.Instance;
            var configuration = new TestDeftConfiguration();
            var sut = new DeftAccessTokenProvider(configuration, clock);

            // Act
            var token = await sut.GetAccessToken();

            // Assert
            Assert.NotNull(token);
        }
    }
}
