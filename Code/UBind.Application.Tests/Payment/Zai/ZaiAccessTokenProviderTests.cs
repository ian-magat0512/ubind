// <copyright file="ZaiAccessTokenProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Zai
{
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Payment.Zai;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests;
    using Xunit;

    public class ZaiAccessTokenProviderTests
    {
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetAccessTokenAsync_ReturnNewToken_WhenConfigIsCorrect()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var configuration = new TestZaiConfiguration();
            var sut = new ZaiAccessTokenProvider(configuration, SystemClock.Instance);

            // Act
            var token = await sut.GetAccessTokenAsync();

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetAccessTokenAsync_ReturnCachedToken_WhenOneExists()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var configuration = new TestZaiConfiguration();
            var sut = new ZaiAccessTokenProvider(configuration, SystemClock.Instance);
            var retrievedToken = await sut.GetAccessTokenAsync();

            // Act
            var cachedToken = await sut.GetAccessTokenAsync();

            // Assert
            cachedToken.Should().NotBeNullOrEmpty();
            cachedToken.Should().BeEquivalentTo(retrievedToken);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetAccessTokenAsync_ThrowProperError_WhenConfigIsWrong()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var configuration = TestZaiConfiguration.GetIncorrectZaiConfiguration();
            var sut = new ZaiAccessTokenProvider(configuration, SystemClock.Instance);

            // Act
            var ex = await Assert.ThrowsAsync<ErrorException>(() => sut.GetAccessTokenAsync());

            // Assert
            ex.Should().NotBeNull();
            var expectedError = Errors.Payment.CouldNotObtainAccessToken("sample error");
            ex.Error.Code.Should().Be(expectedError.Code);
            ex.Error.Title.Should().Be(expectedError.Title);
        }
    }
}
