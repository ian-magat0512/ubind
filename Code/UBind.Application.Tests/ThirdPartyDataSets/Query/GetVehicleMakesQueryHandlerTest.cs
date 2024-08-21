// <copyright file="GetVehicleMakesQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Query
{
    using System.Collections.Generic;
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

    public class GetVehicleMakesQueryHandlerTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleMakesQueryHandlerTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>, GetVehicleMakesQueryHandler>();
            services.AddSingleton(new Mock<IRedBookRepository>().Object);
            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleMakes_GetVehicleMakesQuery()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>>();

            // Act
            var result = await sut.Handle(new GetVehicleMakesQuery(0), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleMakes_GetVehicleMakesQueryWithYearGroup()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>>();

            // Act
            var result = await sut.Handle(new GetVehicleMakesQuery(2020), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
