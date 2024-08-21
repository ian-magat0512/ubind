// <copyright file="TinyUrlServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class TinyUrlServiceTests
    {
        private const string AppBaseUrl = "https://www.ubind.com/";
        private readonly IClock clock = new TestClock();
        private readonly Mock<ITinyUrlRepository> tinyUrlRepositoryMock = new Mock<ITinyUrlRepository>();
        private readonly Mock<IUrlTokenGenerator> urlTokenGeneratorMock = new Mock<IUrlTokenGenerator>();
        private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
        private readonly Mock<IBaseUrlResolver> baseUrlResolver = new Mock<IBaseUrlResolver>();

        public TinyUrlServiceTests()
        {
            this.clock = new TestClock();
            this.baseUrlResolver.Setup(x => x.GetBaseUrl(It.IsAny<Tenant>())).Returns(AppBaseUrl);
        }

        [Theory]
        [InlineData("ihkfsi5d", "https://www.test-website.com/t/g/i/f", "https://www.test-website.com/t/g/i/f")]
        [InlineData("iudne5d2", "/t/g/i/f", $"{AppBaseUrl}t/g/i/f")]
        [InlineData("iudnesd2", $"{AppBaseUrl}t/g/i/f", $"{AppBaseUrl}t/g/i/f")]
        public async Task GetRedirectUrl_ReturnsAbsoluteUrl(string token, string matchingUrl, string expectedUrl)
        {
            // Arrange
            var clock = new TestClock();
            var tinyUrl = new TinyUrl(Guid.NewGuid(), DeploymentEnvironment.Development, matchingUrl, token, 1, clock.Now());
            this.tinyUrlRepositoryMock.Setup(x => x.GetByToken(token)).Returns(Task.FromResult<TinyUrl?>(tinyUrl));
            var sut = new TinyUrlService(
                this.urlTokenGeneratorMock.Object, this.tinyUrlRepositoryMock.Object, this.clock, this.baseUrlResolver.Object, this.cachingResolverMock.Object);

            // Act
            var result = await sut.GetRedirectUrl(token);

            // Assert
            result.Should().Be(expectedUrl);
        }
    }
}
