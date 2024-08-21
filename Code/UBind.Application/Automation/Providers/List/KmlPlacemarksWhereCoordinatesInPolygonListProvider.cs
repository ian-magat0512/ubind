// <copyright file="KmlPlacemarksWhereCoordinatesInPolygonListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using MorseCode.ITask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using SharpKml.Dom;
    using SharpKml.Engine;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.List.Model;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Geometries = NetTopologySuite.Geometries;

    /// <summary>
    /// For providing Kml Placemarks given the necessary arguments.
    /// </summary>
    public class KmlPlacemarksWhereCoordinatesInPolygonListProvider : IDataListProvider<object>
    {
        private List<string> additionalDetails = new List<string>();
        private IProvider<Data<decimal>> longitudeProvider;
        private IProvider<Data<decimal>> latitudeProvider;
        private IProvider<Data<string>> kmlDataProvider;
        private JsonSerializerSettings jsonSettings;
        private dynamic debugContext;
        private XNamespace xNamespace = "http://www.opengis.net/kml/2.2";

        /// <summary>
        /// Initializes a new instance of the <see cref="KmlPlacemarksWhereCoordinatesInPolygonListProvider"/> class.
        /// </summary>
        public KmlPlacemarksWhereCoordinatesInPolygonListProvider(
            IProvider<Data<decimal>> latitudeProvider,
            IProvider<Data<decimal>> longitudeProvider,
            IProvider<Data<string>> kmlDataProvider)
        {
            this.longitudeProvider = longitudeProvider;
            this.latitudeProvider = latitudeProvider;
            this.kmlDataProvider = kmlDataProvider;
        }

        /// <inheritdoc/>
        public List<string> IncludedProperties { get; set; } = new List<string>();

        public string SchemaReferenceKey => "kmlPlacemarksWhereCoordinatesInPolygonList";

        /// <inheritdoc/>
        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            this.jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            var resolveLatitude = (await this.latitudeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var latitude = (double)resolveLatitude.DataValue;
            var resolveLongitude = (await this.longitudeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var longitude = (double)resolveLongitude.DataValue;
            var kmlData = (await this.kmlDataProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            this.debugContext = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            this.debugContext.Add("latitude", latitude);
            this.debugContext.Add("longitude", longitude);
            this.additionalDetails = new List<string>()
            {
                $"KML Data: {kmlData.DataValue}",
            };

            var placemarks = this.GetPlacemarks(new Geometries.Point(longitude, latitude), kmlData.DataValue);

            if (placemarks == null || placemarks.Count == 0)
            {
                throw new ErrorException(Errors.Automation.Provider.KmlPlacemarksNotFound(latitude, longitude, this.additionalDetails, this.debugContext));
            }

            List<object> dictionaryList = new List<object>();

            foreach (var placemark in placemarks)
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(placemark.Json, new AutomationDictionaryConverter());
                dictionaryList.Add(dictionary);
            }

            return ProviderResult<IDataList<object>>.Success(new GenericDataList<object>(dictionaryList));
        }

        private IEnumerable<PlacemarkData> RetrievePlacemarksFromKmlData(string kmlData)
        {
            using (MiniProfiler.Current.Step(nameof(KmlPlacemarksWhereCoordinatesInPolygonListProvider) + "." + nameof(this.RetrievePlacemarksFromKmlData)))
            {
                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var writer = new StreamWriter(stream);
                        writer.Write(kmlData);
                        writer.Flush();
                        stream.Position = 0;
                        var file = KmlFile.Load(stream);
                        var kml = file.Root as Kml;

                        var placemarks = new List<PlacemarkData>();
                        if (kml == null)
                        {
                            return placemarks;
                        }

                        int index = 0;
                        foreach (var placemark in kml.Flatten().OfType<Placemark>())
                        {
                            var points = new List<Geometries.Coordinate>();
                            var coordinateCollection = placemark.Geometry.Flatten().OfType<CoordinateCollection>();
                            foreach (var coordinates in coordinateCollection)
                            {
                                var vectorCollection = coordinates.AsQueryable().ToList();
                                foreach (var vector in vectorCollection)
                                {
                                    points.Add(
                                        new Geometries.Coordinate(
                                        vector.Longitude,
                                        vector.Latitude));
                                }
                            }

                            Geometries.Polygon polygon = null;
                            if (points.Any())
                            {
                                var linearRing = new Geometries.LinearRing(points.ToArray());
                                polygon = new Geometries.Polygon(linearRing);
                            }

                            placemarks.Add(
                                new PlacemarkData(
                                    polygon,
                                    null,
                                    placemark.Description.Text,
                                    index));

                            index++;
                        }

                        return placemarks;
                    }
                    catch (Exception ex)
                    {
                        this.debugContext.Add(Enums.ErrorDataKey.ErrorMessage, ex.Message ?? ex.InnerException?.Message);
                        throw new ErrorException(Errors.Automation.Provider.InvalidKmlData(this.additionalDetails, this.debugContext));
                    }
                }
            }
        }

        private string TransformXmlElementToJson(XElement xElement, string overrideDescription)
        {
            using (MiniProfiler.Current.Step(nameof(KmlPlacemarksWhereCoordinatesInPolygonListProvider) + "." + nameof(this.TransformXmlElementToJson)))
            {
                var xmlString = xElement.ToString();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                string json = JsonConvert.SerializeXmlNode(doc);
                ExpandoObject tmpObj = JsonConvert.DeserializeObject<ExpandoObject>(json);
                var placemarkDic = (IDictionary<string, object>)((IDictionary<string, object>)tmpObj)["Placemark"];
                placemarkDic["description"] = overrideDescription;
                placemarkDic.Remove("@xmlns");
                return JsonConvert.SerializeObject(placemarkDic, this.jsonSettings);
            }
        }

        private List<PlacemarkData> GetPlacemarks(Geometries.Point point, string kmlData)
        {
            var placemarkMaps = this.RetrievePlacemarksFromKmlData(kmlData);

            using (MiniProfiler.Current.Step(nameof(KmlPlacemarksWhereCoordinatesInPolygonListProvider) + "." + nameof(this.GetPlacemarks)))
            {
                var filteredMaps = placemarkMaps.Where(r => r.Polygon != null && r.Polygon.Contains(point));

                if (filteredMaps.Any())
                {
                    using (var stream = new MemoryStream())
                    {
                        var writer = new StreamWriter(stream);
                        writer.Write(kmlData);
                        writer.Flush();
                        stream.Position = 0;
                        var xmlDoc = XDocument.Load(stream);

                        var query = xmlDoc.Root
                               .Element(this.xNamespace + "Document")
                               .Elements(this.xNamespace + "Placemark").ToList();

                        filteredMaps = filteredMaps.Select(x =>
                        {
                            var json = this.TransformXmlElementToJson(query[x.Index], x.Description);
                            return new PlacemarkData(x.Polygon, json, x.Description, x.Index);
                        });
                    }
                }

                return filteredMaps.ToList();
            }
        }
    }
}
