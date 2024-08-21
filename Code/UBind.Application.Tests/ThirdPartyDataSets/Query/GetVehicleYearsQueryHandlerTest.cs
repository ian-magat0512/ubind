// <copyright file="GetVehicleYearsQueryHandlerTest.cs" company="uBind">
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
    using Xunit;

    public class GetVehicleYearsQueryHandlerTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleYearsQueryHandlerTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleYearsQuery, IReadOnlyList<int>>, GetVehicleYearsQueryHandler>();

            var mockRedBookRepository = new Mock<IRedBookRepository>();
            mockRedBookRepository.Setup(m =>
                m.GetVehicleYearsByMakeCodeAndFamilyCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(() => new List<int>()
                    {
                        2021,
                        2011,
                    });

            mockRedBookRepository.Setup(m =>
                    m.GetVehicleYearsByMakeCodeAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(() => new List<int>()
                {
                    2021,
                    2011,
                });

            services.AddSingleton(mockRedBookRepository.Object);

            this.serviceCollection = services;
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleYears_WhenCallingGetVehicleYearsQueryWithFamilyCode()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleYearsQuery, IReadOnlyList<int>>>();

            // Act
            var result = await sut.Handle(new GetVehicleYearsQuery("TOYO", "86"), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsListOfVehicleYears_WhenCallingGetVehicleYearsQuery()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetVehicleYearsQuery, IReadOnlyList<int>>>();

            // Act
            var result = await sut.Handle(new GetVehicleYearsQuery("TOYO", string.Empty), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
        }
    }
}
