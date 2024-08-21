// <copyright file="SearchAustralianBusinessRegisterByNameQueryHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.AbnLookup
{
    using System.Net;
    using System.Threading;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Queries.AbnLookup;
    using UBind.Domain.AbnLookup;
    using Xunit;

    public class SearchAustralianBusinessRegisterByNameQueryHandlerTests
    {
        private readonly Mock<IAbnLookupConfiguration> abnLookupConfiguration = new Mock<IAbnLookupConfiguration>();

        public SearchAustralianBusinessRegisterByNameQueryHandlerTests()
        {
            this.abnLookupConfiguration.Setup(a => a.EndpointConfigurationNameRpc).Returns("ABRXMLSearchRPCSoap");
            this.abnLookupConfiguration.Setup(a => a.UBindGuid).Returns("32c86fe5-b280-4e33-bbd9-68c9b87ede6a");
        }

        [InlineData("aptiture", 10)]
        [InlineData("ubind", 50)]
        [InlineData("corporate", 100)]
        [InlineData("consulting", 500)]
        [InlineData("com", 500)]
        [InlineData("food", 500)]
        [InlineData("financial", 510)]
        [InlineData("design", 520)]
        [InlineData("construction", 530)]
        [InlineData("marketing", 540)]
        [InlineData("north", 500)]
        [InlineData("east", 600)]
        [InlineData("south", 700)]
        [InlineData("west", 800)]
        [InlineData("bank", 900)]
        [InlineData("the", 1000)]
        [InlineData("office", 2000)]
        [InlineData("new", 3000)]
        [InlineData("sales", 4000)]
        [InlineData("australia", 5000)]
        [InlineData("road", 10000)]
        [InlineData("micro", 550)]
        [InlineData("ltd", 560)]
        [InlineData("stock", 570)]
        [InlineData("virgin", 580)]
        [InlineData("sydney", 590)]
        [InlineData("wood", 600)]
        [InlineData("group", 610)]
        [InlineData("airport", 620)]
        [InlineData("hospital", 630)]
        [InlineData("resources", 640)]
        [InlineData("lease", 650)]
        [InlineData("cash", 660)]
        [InlineData("science", 670)]
        [InlineData("automation", 680)]
        [InlineData("land", 690)]
        [InlineData("retail", 700)]
        [InlineData("star", 710)]
        [InlineData("telstra", 720)]
        [InlineData("holdings", 730)]
        [InlineData("telecom", 740)]
        [InlineData("urban", 750)]
        [InlineData("treasury", 760)]
        [InlineData("energy", 770)]
        [InlineData("sports", 780)]
        [InlineData("tri", 790)]
        [InlineData("village", 800)]
        [InlineData("movie", 810)]
        [InlineData("travel", 820)]
        [InlineData("lite", 830)]
        [InlineData("close", 840)]
        [InlineData("friend", 850)]
        [InlineData("big", 860)]
        [InlineData("team", 870)]
        [InlineData("xero", 880)]
        [InlineData("pool", 890)]
        [InlineData("beach", 900)]
        [InlineData("challenger", 910)]
        [InlineData("coca", 920)]
        [InlineData("cola", 930)]
        [InlineData("common", 940)]
        [InlineData("computer", 950)]
        [InlineData("share", 960)]
        [InlineData("data", 970)]
        [InlineData("domino", 980)]
        [InlineData("drinks", 990)]
        [InlineData("pizza", 1000)]
        [InlineData("bags", 1010)]
        [InlineData("royal", 1020)]
        [InlineData("red", 1030)]
        [InlineData("green", 1040)]
        [InlineData("blue", 1050)]
        [InlineData("sun", 1060)]
        [InlineData("moon", 1070)]
        [InlineData("earth", 1080)]
        [InlineData("wind", 1090)]
        [InlineData("fire", 1100)]
        [InlineData("light", 1110)]
        [InlineData("cloud", 1120)]
        [InlineData("forest", 1130)]
        [InlineData("tree", 1140)]
        [InlineData("chocolate", 1150)]
        [InlineData("factory", 1160)]
        [InlineData("box", 1170)]
        [InlineData("apple", 1180)]
        [InlineData("mac", 1190)]
        [InlineData("corner", 1200)]
        [InlineData("nike", 1210)]
        [InlineData("adidas", 1220)]
        [InlineData("new", 1230)]
        [InlineData("balance", 1240)]
        [InlineData("asics", 1250)]
        [InlineData("industries", 1260)]
        [InlineData("global", 1270)]
        [InlineData("insurance", 1280)]
        [InlineData("tech", 1290)]
        [InlineData("leon", 1300)]
        [InlineData("carl", 1310)]
        [InlineData("sage", 1320)]
        [InlineData("gamble", 1330)]
        [Theory(Skip = "This test takes around 6 minutes. Reenable this if you change the code related to this.")]

        public async void Handle_ReturnsSuccess_WhenSearchingWithMaxResults(string search, int maxResults)
        {
            // Arrange
            var handler = new SearchAustralianBusinessRegisterByNameQueryHandler(this.abnLookupConfiguration.Object);
            var request = new SearchAustralianBusinessRegisterByNameQuery(search, maxResults, true, true, true, false);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Asssert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async void Handle_ReturnsFailure_WhenNoNametypesIncludedInSearch()
        {
            // Arrange
            var handler = new SearchAustralianBusinessRegisterByNameQueryHandler(this.abnLookupConfiguration.Object);
            var request = new SearchAustralianBusinessRegisterByNameQuery("Leon", 10, false, false, false, false);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Asssert
            result.IsFailure.Should().BeTrue();
            result.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Error.Code.Should().Be("abr.no.nametypes.included.in.search");
        }
    }
}
