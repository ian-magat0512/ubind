// <copyright file="PremiumFundingAccessTokenProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.PremiumFunding
{
    using System.Net;
    using System.Threading.Tasks;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Domain.Tests;
    using Xunit;

    public class PremiumFundingAccessTokenProviderTests
    {
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task GetAccessToken_ReturnsValidToken_WhenCredentialsAreValid()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var sut = new AccessTokenProvider();

            // Act
            var accessToken = await sut.GetAccessToken("aptiture-dev", "hwyRrL4hU1nU", "1.4.9");

            // Assert
            Assert.NotNull(accessToken);
        }
    }
}
