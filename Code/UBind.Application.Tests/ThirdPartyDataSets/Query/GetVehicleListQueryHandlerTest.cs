// <copyright file="GetVehicleListQueryHandlerTest.cs" company="uBind">
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

    public class GetVehicleListQueryHandlerTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleListQueryHandlerTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleListQuery, IReadOnlyList<Vehicle>>, GetVehicleListQueryHandler>();

            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleFamilies_WhenCallingGetVehicleListAsync()
        {
            // Arrange
            var mockRedBookRepository = new Mock<IRedBookRepository>();
            mockRedBookRepository.Setup(m =>
                    m.GetVehicleListAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<string?>(),
                        It.IsAny<string?>(),
                        It.IsAny<string?>()))
                .ReturnsAsync(() => new List<Vehicle>()
                {
                    new Vehicle()
                    {
                        VehicleKey = "AUVTOYO2017AEAK",
                        VehicleDescription = "ASV50R Atara SL Sedan 4dr Spts Auto 6sp 2.5i",
                        MakeCode = "TOYO",
                        MakeDescription = "Toyota",
                        FamilyCode = "CAMRY",
                        FamilyDescription = "Camry",
                        YearGroup = 2017,
                    },
                    new Vehicle()
                    {
                        VehicleKey = "AUVTOYO2018AEIE",
                        VehicleDescription = "GSV70R SL Sedan 4dr Spts Auto 8sp 3.5i",
                        MakeCode = "TOYO",
                        MakeDescription = "Toyota",
                        FamilyCode = "CAMRY",
                        FamilyDescription = "Camry",
                        YearGroup = 2017,
                    },
                });

            this.serviceCollection.AddSingleton(mockRedBookRepository.Object);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleListQuery, IReadOnlyList<Vehicle>>>();

            // Act
            var result = await sut.Handle(new GetVehicleListQuery("TOYO", "86", 2017, string.Empty, string.Empty, string.Empty), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }
    }
}
