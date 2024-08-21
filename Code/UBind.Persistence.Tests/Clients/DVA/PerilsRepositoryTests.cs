// <copyright file="PerilsRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Clients.DVA
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using UBind.Domain.Clients.DVA.Perils.Entities;
    using UBind.Persistence.Clients.DVA.Migrations;
    using UBind.Persistence.Clients.DVA.Perils.Respositories;
    using Xunit;

    public class PerilsRepositoryTests
    {
        [Fact]
        public void PerilsRepository_GetLatestPeril_ShouldReturnRecordWithLatestEffectivityDate()
        {
            string gnafPid = "TEST_GNAF_ID0001";

            // Arrange
            Peril oldPeril = new Peril
            {
                GnafPid = gnafPid,
                IcaZone = "Z001",
                PostCode = "4000",
                CycloneRate = 0.000001d,
                FireRate = 0.000002d,
                FloodRate = 0.000003d,
                QuakeRate = 0.000004d,
                StormRate = 0.000005d,
                TotalRate = 0.000006d,
                EffectiveDate = new System.DateTime(2020, 10, 3),
            };
            Peril latestPeril = new Peril
            {
                GnafPid = gnafPid,
                IcaZone = "Z001",
                PostCode = "4000",
                CycloneRate = 0.000001d,
                FireRate = 0.000002d,
                FloodRate = 0.000003d,
                QuakeRate = 0.000004d,
                StormRate = 0.000005d,
                TotalRate = 0.000006d,
                EffectiveDate = new System.DateTime(2021, 10, 3),
            };

            var data = new List<Peril>()
            {
                oldPeril,
                latestPeril,
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Peril>>();
            mockSet.As<IQueryable<Peril>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Peril>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Peril>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Peril>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.Setup(x => x.AsNoTracking()).Returns(mockSet.Object);

            var mockContext = new Mock<DvaDbContext>();
            mockContext.Setup(p => p.Perils).Returns(mockSet.Object);

            var perilRep = new PerilsRepository(mockContext.Object);
            var item = perilRep.GetDetailsByPropertyId(gnafPid);

            // Assert
            item.Should().NotBeNull();
            item.Should().Be(latestPeril);
        }
    }
}
