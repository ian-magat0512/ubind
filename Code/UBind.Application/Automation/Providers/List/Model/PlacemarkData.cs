// <copyright file="PlacemarkData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List.Model
{
    using NetTopologySuite.Geometries;
    using Newtonsoft.Json;

    /// <summary>
    /// Placemark data model to be able to search through json content using polygons.
    /// </summary>
    public class PlacemarkData
    {
        public PlacemarkData(Polygon polygon, string placemarkJson, string overrideDescription, int index)
        {
            this.Polygon = polygon;
            this.Json = placemarkJson;
            this.Description = overrideDescription;
            this.Index = index;
        }

        [JsonIgnore]
        public Polygon Polygon { get; set; }

        public string Json { get; set; }

        public string Description { get; }

        public int Index { get; }
    }
}
