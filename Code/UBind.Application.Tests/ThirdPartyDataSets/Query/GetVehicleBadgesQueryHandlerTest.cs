// <copyright file="GetVehicleBadgesQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.ThirdPartyDataSets.Query;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.RedBook;
using Xunit;

public class GetVehicleBadgesQueryHandlerTest
{
    private readonly ServiceCollection serviceCollection;

    public GetVehicleBadgesQueryHandlerTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<GetVehicleBadgesQuery, IEnumerable<VehicleBadge>>, GetVehicleBadgesQueryHandler>();

        this.serviceCollection = services;
    }

    [Fact]
    public async Task Handle_ReturnsListOfVehicleBadges_WhenCallingGetVehicleBodyStyleByMakeCodeFamilyCodeYearBodyStyleAndGearType()
    {
        // Arrange
        var mockRedBookRepository = new Mock<IRedBookRepository>();
        mockRedBookRepository.Setup(m =>
                m.GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType(
                     "TOYO",
                    "LANDCRU",
                    2019,
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
            .ReturnsAsync(() => new List<VehicleBadge>()
            {
                new VehicleBadge()
                {
                    MakeCode = "TOYO",
                    FamilyCode = "LANDCRU",
                    VehicleTypeCode = "LC",
                    Description = "GX",
                    SecondaryDescription = string.Empty,
                },
                  new VehicleBadge()
                {
                    MakeCode = "TOYO",
                    FamilyCode = "LANDCRU",
                    VehicleTypeCode = "SV",
                    Description = "GX",
                    SecondaryDescription = string.Empty,
                },
                    new VehicleBadge()
                {
                    MakeCode = "TOYO",
                    FamilyCode = "LANDCRU",
                    VehicleTypeCode = "SV",
                    Description = "GXL",
                    SecondaryDescription = "Troopcarrier",
                },
                    new VehicleBadge()
                {
                    MakeCode = "TOYO",
                    FamilyCode = "LANDCRU",
                    VehicleTypeCode = "SV",
                    Description = "Sahara",
                    SecondaryDescription = "Horizon",
                    },
            });

        this.serviceCollection.AddSingleton(mockRedBookRepository.Object);

        var service = this.serviceCollection.BuildServiceProvider();
        var sut = service.GetService<IQueryHandler<GetVehicleBadgesQuery, IEnumerable<VehicleBadge>>>();

        if (sut != null)
        {

            // Act
            var result = await sut.Handle(
                new GetVehicleBadgesQuery(
                    "TOYO", "LANDCRU", 2019, string.Empty, string.Empty), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(4);
        }
    }
}
