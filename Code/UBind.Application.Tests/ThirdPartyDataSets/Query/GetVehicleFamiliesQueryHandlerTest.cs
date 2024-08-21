// <copyright file="GetVehicleFamiliesQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.RedBook;
    using Xunit;

    public class GetVehicleFamiliesQueryHandlerTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleFamiliesQueryHandlerTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleFamiliesQuery, IEnumerable<VehicleFamily>>, GetVehicleFamiliesQueryHandler>();

            var mockRedBookRepository = new Mock<IRedBookRepository>();

            mockRedBookRepository.Setup(m =>
                m.GetVehicleFamiliesByMakeCodeAndYearGroupAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(() => new List<VehicleFamily>()
                    {
                         new VehicleFamily()
                         {
                             FamilyCode = "500",
                             MakeCode = "TOYO",
                         },
                         new VehicleFamily()
                         {
                             FamilyCode = "600",
                             MakeCode = "TOYO",
                         },
                    });

            mockRedBookRepository.Setup(m =>
                    m.GetVehicleFamiliesByMakeCodeAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(() => new List<VehicleFamily>()
                {
                    new VehicleFamily()
                    {
                        FamilyCode = "500",
                        MakeCode = "TOYO",
                    },
                    new VehicleFamily()
                    {
                        FamilyCode = "600",
                        MakeCode = "TOYO",
                    },
                });

            services.AddSingleton(mockRedBookRepository.Object);

            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleFamilies_WhenCallingGetVehicleFamiliesQueryByMakeCodeAndYearGroup()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleFamiliesQuery, IEnumerable<VehicleFamily>>>();

            // Act
            var result = await sut.Handle(new GetVehicleFamiliesQuery("TOYO", 2002), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleFamilies_WhenCallingGetVehicleFamiliesQueryByMakeCode()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleFamiliesQuery, IEnumerable<VehicleFamily>>>();

            // Act
            var result = await sut.Handle(new GetVehicleFamiliesQuery("TOYO", 0), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }
    }
}
