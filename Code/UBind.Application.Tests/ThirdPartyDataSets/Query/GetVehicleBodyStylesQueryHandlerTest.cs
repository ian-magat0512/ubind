// <copyright file="GetVehicleBodyStylesQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.ThirdPartyDataSets.Query;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using Xunit;

public class GetVehicleBodyStylesQueryHandlerTest
{
    private readonly ServiceCollection serviceCollection;

    public GetVehicleBodyStylesQueryHandlerTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<GetVehicleBodyStylesQuery, IEnumerable<string>>, GetVehicleBodyStylesQueryHandler>();

        var mockRedBookRepository = new Mock<IRedBookRepository>();
        mockRedBookRepository.Setup(m =>
                m.GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType(
                     "TOYO",
                    "CAMRY",
                    2018,
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
            .ReturnsAsync(() => new List<string>()
            { "Sedan", });

        services.AddSingleton(mockRedBookRepository.Object);

        this.serviceCollection = services;
    }

    [Fact]
    public async Task Handle_ReturnsListOfVehicleBodyStyles_WhenCallingGetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType()
    {
        // Arrange
        var service = this.serviceCollection.BuildServiceProvider();
        var sut = service.GetService<IQueryHandler<GetVehicleBodyStylesQuery, IEnumerable<string>>>();

        if (sut != null)
        {
            // Act
            var result = await sut.Handle(
                new GetVehicleBodyStylesQuery("TOYO", "CAMRY", 2018, string.Empty, string.Empty), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
        }
    }
}