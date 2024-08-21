// <copyright file="PerilsServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Clients.DVA.Perils.Services
{
    using Moq;
    using UBind.Application.Clients.DVA.Perils.Services;
    using UBind.Domain.Clients.DVA.Perils.Entities;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;
    using Xunit;

    public class PerilsServiceTests
    {
        /// <summary>
        /// Defines the perils repository.
        /// </summary>
        private Mock<IPerilsRepository> perilsRepository;

        public PerilsServiceTests()
        {
            this.perilsRepository = new Mock<IPerilsRepository>();
        }

        [Fact]
        public void Perils_GetDetailsById_ShouldReturnPerilDetails()
        {
            Peril peril = new Peril
            {
                GnafPid = "TEST_GNAF_ID0001",
                IcaZone = "Z001",
                PostCode = "4000",
                CycloneRate = 0.000001d,
                FireRate = 0.000002d,
                FloodRate = 0.000003d,
                QuakeRate = 0.000004d,
                StormRate = 0.000005d,
                TotalRate = 0.000006d,
            };

            this.perilsRepository.Setup(p => p.GetDetailsByPropertyId("TEST_GNAF_ID0001")).Returns(peril);

            var perilService = new PerilsService(this.perilsRepository.Object);

            // act
            var perilDetails = perilService.GetDetailsByPropertyId("TEST_GNAF_ID0001");

            // assert
            Assert.Equal("Z001", perilDetails.IcaZone);
            Assert.Equal("4000", perilDetails.PostCode);
            Assert.Equal(0.000001d, perilDetails.CycloneRate);
            Assert.Equal(0.000002d, perilDetails.FireRate);
            Assert.Equal(0.000003d, perilDetails.FloodRate);
            Assert.Equal(0.000004d, perilDetails.QuakeRate);
            Assert.Equal(0.000005d, perilDetails.StormRate);
            Assert.Equal(0.000006d, perilDetails.TotalRate);
        }

        [Fact]
        public void Perils_GetDetailsById_ShouldReturnNull_WhenIdIsNotExisting()
        {
            Peril peril = new Peril
            {
                GnafPid = "TEST_GNAF_ID0001",
                IcaZone = "Z001",
                PostCode = "4000",
                CycloneRate = 2.087269E-10,
                FireRate = 0.000002d,
                FloodRate = 0.000003d,
                QuakeRate = 0.000004d,
                StormRate = 0.000005d,
                TotalRate = 0.000006d,
            };

            this.perilsRepository.Setup(p => p.GetDetailsByPropertyId("TEST_GNAF_ID0001")).Returns(peril);

            var perilService = new PerilsService(this.perilsRepository.Object);

            // act
            var perilDetails = perilService.GetDetailsByPropertyId("TEST_GNAF_ID0002");

            // assert
            Assert.Null(perilDetails);
        }
    }
}
