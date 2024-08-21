// <copyright file="KmlPlacemarksWhereCoordinatesInPolygonListProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class KmlPlacemarksWhereCoordinatesInPolygonListProviderTests
    {
        private string klmTestFile = "Automations\\Providers\\List\\TestFiles\\rainfall.kml";

        [Theory]
        [InlineData(-42.2790632, 145.45062, ">2000")]
        [InlineData(-40.4509606, 144.9447105, "600-1000")]
        [InlineData(-38.5907658, 144.9887311, "600-1000")]
        [InlineData(-40.4529587, 144.9466221, "600-1000")]
        [InlineData(-21.2328939, 143.8427554, "400-600")]
        [InlineData(-21.2328939, 143.8427555, "400-600")]
        [InlineData(-22.0115444, 132.7391026, "300-400")]
        [InlineData(-26.8870396, 140.5459121, "200-300")]
        [InlineData(-37.6958871, 147.9097662, "600-1000")]
        public async Task KmlPlacemarksWhereCoordinatesInPolygonListProvider_ShouldCreateDataObject_WithProperInput(
            decimal latitude,
            decimal longitude,
            string expectedRainfallResult)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var xmlContent = File.ReadAllText(this.klmTestFile);
            var provider = this.BuildProvider(latitude, longitude, xmlContent);

            // Act
            var resolveDataObject = await provider.Resolve(new ProviderContext(automationData));
            IQueryable<dynamic> dataObject = resolveDataObject.GetValueOrThrowIfFailed().Query;

            // Assert
            List<PlacemarkTmp> placemarks = new List<PlacemarkTmp>();

            foreach (var item in dataObject)
            {
                var rainFall = DataObjectHelper.GetPropertyValue(item, "name");
                var desc = DataObjectHelper.GetPropertyValue(item, "description");
                placemarks.Add(new PlacemarkTmp
                {
                    Description = desc,
                    Region = int.Parse(rainFall),
                    Obj = item,
                });
            }

            var val = placemarks.OrderByDescending(x => x.Region).First();
            dataObject.Count().Should().BeGreaterThan(1);
            expectedRainfallResult.Should().Be(val.Description);
        }

        [Theory]
        [InlineData(-412.2790632, 145.45062)]
        public async Task KmlPlacemarksWhereCoordinatesInPolygonListProvider_ShouldCauseError_IfCoordinatesNoMatch(
            decimal latitude,
            decimal longitude)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var xmlContent = File.ReadAllText(this.klmTestFile);
            var provider = this.BuildProvider(latitude, longitude, xmlContent);

            // Act
            Func<Task> func = async () => await provider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.kml.placemarks.not.found");
        }

        [Theory]
        [InlineData(-42.2790632, 145.45062)]
        public async Task KmlPlacemarksWhereCoordinatesInPolygonListProvider_ShouldCauseError_IfKMLDataIsInvalid(
            decimal latitude,
            decimal longitude)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var xmlContent = "This is an invalid KML Data";
            var provider = this.BuildProvider(latitude, longitude, xmlContent);

            // Act
            Func<Task> func = async () => await provider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.kml.placemark.invalid.kmldata");
        }

        private IDataListProvider<object> BuildProvider(decimal latitude, decimal longitude, string kmlData)
        {
            var longitudeDataProvider = new StaticBuilder<Data<decimal>>() { Value = longitude };
            var latitudeDataProvider = new StaticBuilder<Data<decimal>>() { Value = latitude };
            var klmDataProvider = new StaticBuilder<Data<string>>() { Value = kmlData };
            var providerModel = new KmlPlacemarksWhereCoordinatesInPolygonListProviderConfigModel()
            {
                Longitude = longitudeDataProvider,
                Latitude = latitudeDataProvider,
                KmlData = klmDataProvider,
            };

            var objectProvider = providerModel.Build(null);
            return objectProvider;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class PlacemarkTmp
#pragma warning restore SA1402 // File may only contain a single type
    {
        public int Region { get; set; }

        public object Obj { get; set; }

        public string Description { get; set; }
    }
}
