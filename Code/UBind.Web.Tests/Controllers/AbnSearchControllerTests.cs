// <copyright file="AbnSearchControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method should be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Queries.AbnLookup;
    using UBind.Domain;
    using UBind.Domain.AbnLookup;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Controllers;
    using UBind.Web.Infrastructure;
    using Xunit;

    public class AbnSearchControllerTests
    {
        private readonly Mock<ICqrsMediator> mediator;

        public AbnSearchControllerTests()
        {
            this.mediator = new Mock<ICqrsMediator>();
        }

        [Fact]
        public async void SearchByAbn_ReturnsOkResult_WhenAbnParameterIsProvided()
        {
            // Arrange
            var abn = "42008163933";

            var mockedResponse = new AbnSearchResponse(
                abn,
                "active",
                DateTime.Now.AddYears(-1),
                "123123",
                DateTime.Now.AddYears(-1),
                "0322",
                "ACT",
                null,
                "SAMPLE ENTITY NAME",
                "Australian Public Company",
                "PUB",
                null,
                null,
                DateTime.Now.AddYears(-1));

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Success<AbnSearchResponse, Error>(mockedResponse)));

            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var searchResults = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            searchResults.GetType().Should().Be(typeof(OkObjectResult));
        }

        [Fact]
        public async void SearchByAbn_ShouldNotContainGstRegistrationDate_WhenNull()
        {
            // Arrange
            var abn = "42008163933";

            var mockedResponse = new AbnSearchResponse(
                abn,
                "active",
                DateTime.Now.AddYears(-1),
                "123123",
                DateTime.Now.AddYears(-1),
                "0322",
                "ACT",
                null,
                "SAMPLE ENTITY NAME",
                "Australian Public Company",
                "PUB",
                null,
                null,
                null);  // GST

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Success<AbnSearchResponse, Error>(mockedResponse)));

            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var searchResults = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            searchResults.GetType().Should().Be(typeof(OkObjectResult));

            var objectResult = ((ObjectResult)searchResults).Value;

            var json = JsonConvert.SerializeObject(objectResult);
            var jObject = JObject.Parse(json);
            jObject.SelectToken("Acn").Should().NotBeNull();
            jObject.SelectToken("GstRegistrationDate").Should().BeNull();
        }

        [Fact]
        public async void SearchByAbn_ShouldNotContainACN_WhenNull()
        {
            // Arrange
            var abn = "42008163933";
            var dateFrom = DateTime.Now.AddYears(-1);

            var mockedResponse = new AbnSearchResponse(
                abn,
                "active",
                dateFrom,
                null,   // ACN
                dateFrom,
                "0322",
                "ACT",
                null,
                "SAMPLE ENTITY NAME",
                "Australian Public Company",
                "PUB",
                null,
                null,
                dateFrom);  // GST

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Success<AbnSearchResponse, Error>(mockedResponse)));

            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var searchResults = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            searchResults.GetType().Should().Be(typeof(OkObjectResult));

            var objectResult = ((ObjectResult)searchResults).Value;

            var json = JsonConvert.SerializeObject(objectResult);
            var jObject = JObject.Parse(json);
            jObject.SelectToken("GstRegistrationDate").Should().NotBeNull();
            jObject.SelectToken("Acn").Should().BeNull();
        }

        [Fact]
        public async void SearchByAbn_PayloadShouldNotContainProperty_WhenNull()
        {
            // Arrange
            var abn = "42008163933";
            var dateFrom = DateTime.Now.AddYears(-1);

            var mockedResponse =
                new AbnSearchResponse(abn, null, dateFrom, null, dateFrom, null, null, null, null, null, null, null, null, null);

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Success<AbnSearchResponse, Error>(mockedResponse)));

            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var searchResults = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            searchResults.GetType().Should().Be(typeof(OkObjectResult));

            var objectResult = ((ObjectResult)searchResults).Value;

            var json = JsonConvert.SerializeObject(objectResult);
            var jObject = JObject.Parse(json);
            jObject.SelectToken("Acn").Should().BeNull();           // Numeric
            jObject.SelectToken("EntityName").Should().BeNull();    // String
            jObject.SelectToken("BusinessNames").Should().BeNull(); // Array
        }

        [Fact]
        public async void SearchByAbn_ReturnsError_WhenAbnParameterIsInvalid()
        {
            // Arrange
            var abn = "123";

            var dateFrom = DateTime.Now.AddYears(-1);

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    Result.Failure<AbnSearchResponse, Error>(Errors.AbnLookup.InvalidAbn(abn))));
            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var result = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            var jsonResult = result as JsonResult;
            result.Should().NotBeNull();
            var problemDetails = (UBindProblemDetails)jsonResult.Value;
            problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
            problemDetails.Code.Should().Be("abr.invalid.abn");
        }

        [Fact]
        public async void SearchByAbn_ReturnsError_WhenAbnIsNotFound()
        {
            // Arrange
            var abn = "1234568901";

            var dateFrom = DateTime.Now.AddYears(-1);

            var request = new SearchAustralianBusinessRegisterByAbnQuery(abn);
            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByAbnQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    Result.Failure<AbnSearchResponse, Error>(Errors.AbnLookup.AbnNotFound(abn))));
            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var result = await controller.SearchByAbn(abn, CancellationToken.None);

            // Assert
            var jsonResult = result as JsonResult;
            result.Should().NotBeNull();
            var problemDetails = (UBindProblemDetails)jsonResult.Value;
            problemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
            problemDetails.Code.Should().Be("abr.abn.registration.not.found");
        }

        [Fact]
        public async void SearchByName_ReturnsOkResult_WhenSearchParameterIsProvided()
        {
            // Arrange
            int maxResults = 10;
            string search = "aptiture";

            var request = new SearchAustralianBusinessRegisterByNameQuery(search, maxResults, true, true, true, false);

            this.mediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .Returns(this.GetMockedSearchResult(search, maxResults));
            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var searchResults = await controller.SearchByName(search, CancellationToken.None);

            // Assert
            searchResults.GetType().Should().Be(typeof(OkObjectResult));
        }

        [Fact]
        public async Task SearchByName_ThrowsException_WhenSearchParameterIsNotProvided()
        {
            // Arrange
            int maxResults = 10;
            string search = string.Empty;

            var request = new SearchAustralianBusinessRegisterByNameQuery(search, maxResults, true, true, true, false);

            this.mediator.Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
                .Returns(this.GetMockedSearchResult(search, maxResults));

            var controller = new AbnSearchController(this.mediator.Object);

            // Act
            var result = await controller.SearchByName(search, CancellationToken.None, maxResults);

            // Assert
            var jsonResult = result as JsonResult;
            result.Should().NotBeNull();
            var problemDetails = (Infrastructure.UBindProblemDetails)jsonResult.Value;
            problemDetails.Status.Should().Be(400);
            problemDetails.Code.Should().Be("bad.request");
        }

        [Fact]
        public async void SearchByName_ThrowsException_WhenNoNametypesIncludedInSearch()
        {
            // Arrange
            var maxResults = 10;
            var search = "leon";
            bool includeEntityNames = false;
            bool includeBusinessNames = false;
            bool includeTradingNames = false;

            var controller = new AbnSearchController(this.mediator.Object);

            this.mediator.Setup(m => m.Send(
                It.IsAny<SearchAustralianBusinessRegisterByNameQuery>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    Result.Failure<AbnNameSearchResponse, Error>(Errors.AbnLookup.NoNametypesIncludedInSearch())));

            // Act
            var result = await controller.SearchByName(
                search, CancellationToken.None, maxResults, includeEntityNames, includeBusinessNames, includeTradingNames, false);

            // Assert
            var jsonResult = result as JsonResult;
            result.Should().NotBeNull();
            var problemDetails = (UBindProblemDetails)jsonResult.Value;
            problemDetails.Status.Should().Be(400);
            problemDetails.Code.Should().Be("abr.no.nametypes.included.in.search");
        }

        private Task<Result<AbnNameSearchResponse, Error>> GetMockedSearchResult(string search, int maxResults)
        {
            var nameInfoList = new List<AbnRegistration>();
            for (int i = 0; i < maxResults; i++)
            {
                var abnInfo = new AbnRegistration(
                    (10000000000 + i).ToString(),
                    "active",
                    "{search} {i}",
                    "Business Name",
                    322,
                    "ACT",
                    100 - i);
                nameInfoList.Add(abnInfo);
            }

            var response = new AbnNameSearchResponse(nameInfoList);

            return Task.FromResult(Result.Success<AbnNameSearchResponse, Error>(response));
        }
    }
}
