// <copyright file="GetVehicleByVehicleKeyQueryHandlerIntegrationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Query
{
    using System.Threading.Tasks;
    using Dapper;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests;
    using UBind.Domain.ThirdPartyDataSets.RedBook;
    using UBind.Persistence.ThirdPartyDataSets;
    using Xunit;

    public class GetVehicleByVehicleKeyQueryHandlerIntegrationTest
    {
        private readonly ServiceCollection serviceCollection;

        public GetVehicleByVehicleKeyQueryHandlerIntegrationTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryHandler<GetVehicleByVehicleKeyQuery, VehicleDetails>, GetVehicleByVehicleKeyQueryHandler>();
            var appSettingsConfig = new ConfigurationBuilder().AddJsonFile("appsettings.localtest.json").Build();
            var connectionString = appSettingsConfig.GetConnectionString("ThirdPartyDataSets");

            services.AddSingleton<IThirdPartyDataSetsDbConfiguration>(config => new ThirdPartyDataSetsDbConfiguration(connectionString));
            services.AddScoped(ctx => new ThirdPartyDataSetsDbContext(connectionString));
            services.AddSingleton<IThirdPartyDataSetsDbObjectFactory, ThirdPartyDataSetsDbObjectFactory>();

            services.AddSingleton<IRedBookRepository, RedBookRepository>();

            this.serviceCollection = services;
        }

        [Fact(Skip = "VerySlowRequiresFullSqlFeature")]
        [Trait("TestCategory", TestCategory.VerySlowRequiresFullSqlFeature)]
        public async Task Handle_ReturnsVehicleDetails_WhenCallingGetVehicleByVehicleKeyQuery()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();

            var sut = service.GetService<IRedBookRepository>();
            var thirdPartyDataSetsDbObjectFactory = service.GetService<IThirdPartyDataSetsDbObjectFactory>();

            // Act
            using (var connection = thirdPartyDataSetsDbObjectFactory.GetNewConnection(6000))
            {
                var count = 0;
                var vehicleKeys = await connection.QueryAsync<string>("SELECT [VehicleKey] FROM [RedBook].[VEVehicle]");
                foreach (var key in vehicleKeys)
                {
                    var result = await sut.GetVehicleByVehicleKeyAsync(key);
                    //// Assert
                    result.Should().NotBeNull();
                    result.VehicleKey.Should().NotBeNullOrEmpty();
                    count++;
                }

                count.Should().Be(44380);
            }
        }
    }
}
