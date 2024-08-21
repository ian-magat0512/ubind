// <copyright file="Detail.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.Nfid
{
    /// <summary>
    /// Represents the NFID Detail.
    /// </summary>
    public class Detail
    {
        public string GnafAddressId { get; set; }

        public double Elevation { get; set; }

        public double FloodDepth20 { get; set; }

        public double FloodDepth50 { get; set; }

        public double FloodDepth100 { get; set; }

        public double FloodDepthExtreme { get; set; }

        public int FloodAriGl { get; set; }

        public int FloodAriGl1M { get; set; }

        public int FloodAriGl2M { get; set; }

        public int NotesId { get; set; }

        public int LevelNfidId { get; set; }

        public int LevelFezId { get; set; }

        public string FloodCode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
