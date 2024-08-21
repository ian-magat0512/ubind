// <copyright file="GetVehicleByVehicleKeyQueryHandlerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Query
{
    using System;
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

    public class GetVehicleByVehicleKeyQueryHandlerTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleByVehicleKeyQueryHandlerTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleByVehicleKeyQuery, VehicleDetails>, GetVehicleByVehicleKeyQueryHandler>();

            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsVehicleDetails_WhenCallingGetVehicleByVehicleKeyQuery()
        {
            // Arrange
            var mockRedBookRepository = new Mock<IRedBookRepository>();
            mockRedBookRepository.Setup(m =>
                    m.GetVehicleByVehicleKeyAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    var vehicle = new VehicleDetails
                    {
                        VehicleKey = "AUVTOYO2017AEAK",
                        VehicleDescription = "ASV50R Atara SL Sedan 4dr Spts Auto 6sp 2.5i",
                        MakeCode = "TOYO",
                        MakeDescription = "Toyota",
                        FamilyCode = "CAMRY",
                        FamilyDescription = "Camry",
                        YearGroup = 2017,
                    };
                    return vehicle;
                });

            this.serviceCollection.AddSingleton(mockRedBookRepository.Object);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleByVehicleKeyQuery, VehicleDetails>>();

            // Act
            var result = await sut.Handle(new GetVehicleByVehicleKeyQuery("AUVTOYO2017AEAK"), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleKey.Should().Be("AUVTOYO2017AEAK");
            result.YearGroup.Should().Be(2017);
        }

        [Fact]
        public async Task Handle_ThrowsErrorException_WhenCallingGetVehicleByVehicleKeyQuery()
        {
            // Arrange
            var mockRedBookRepository = new Mock<IRedBookRepository>();
            mockRedBookRepository.Setup(m =>
                    m.GetVehicleByVehicleKeyAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(() => null);

            this.serviceCollection.AddSingleton(mockRedBookRepository.Object);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleByVehicleKeyQuery, VehicleDetails>>();

            // Act
            Func<Task> action = async () => await sut.Handle(new GetVehicleByVehicleKeyQuery("AUVTOYO2017AEAK"), CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<UBind.Domain.Exceptions.ErrorException>();
        }
    }
}
