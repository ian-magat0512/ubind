// <copyright file="GetVehicleGearTypesQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.ThirdPartyDataSets.Query;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using Xunit;

public class GetVehicleGearTypesQueryHandlerTest
{
    private readonly ServiceCollection serviceCollection;

    public GetVehicleGearTypesQueryHandlerTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<GetVehicleGearTypesQuery, IEnumerable<string>>, GetVehicleGearTypesQueryHandler>();

        var mockRedBookRepository = new Mock<IRedBookRepository>();
        mockRedBookRepository.Setup(m =>
                m.GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle(
                     "TOYO",
                    "CAMRY",
                    2018,
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync(() => new List<string>()
            { "Constantly Variable Transmission", "Sports Automatic" });

        services.AddSingleton(mockRedBookRepository.Object);

        this.serviceCollection = services;
    }

    [Fact]
    public async Task Handle_ReturnsListOfGearTypes_WhenCallingGetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndBodyStyles()
    {
        // Arrange
        var service = this.serviceCollection.BuildServiceProvider();
        var sut = service.GetService<IQueryHandler<GetVehicleGearTypesQuery, IEnumerable<string>>>();

        if (sut != null)
        {
            // Act
            var result = await sut.Handle(
                new GetVehicleGearTypesQuery("TOYO", "CAMRY", 2018, null, null), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }
    }
}
