// <copyright file="Peril.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Clients.DVA.Perils.Entities
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// DVA Peril Model.
    /// </summary>
    public class Peril
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Peril"/> class.
        /// </summary>
        public Peril()
        {
        }

        /// <summary>
        /// Gets or sets the G-NAF address details Id.
        /// </summary>
        [JsonProperty(PropertyName = "Id")]
        public string GnafPid { get; set; }

        /// <summary>
        /// Gets or sets the Post Code.
        /// </summary>
        public string PostCode { get; set; }

        /// <summary>
        /// Gets or sets the ICA Zone.
        /// </summary>
        public string IcaZone { get; set; }

        /// <summary>
        /// Gets or sets the Storm Rate.
        /// </summary>
        public double StormRate { get; set; }

        /// <summary>
        /// Gets or sets the Cyclone Rate.
        /// </summary>
        public double CycloneRate { get; set; }

        /// <summary>
        /// Gets or sets the Fire Rate.
        /// </summary>
        public double FireRate { get; set; }

        /// <summary>
        /// Gets or sets the Flood Rate.
        /// </summary>
        public double FloodRate { get; set; }

        /// <summary>
        /// Gets or sets the Quake Rate.
        /// </summary>
        public double QuakeRate { get; set; }

        /// <summary>
        /// Gets or sets the Total Rate.
        /// </summary>
        public double TotalRate { get; set; }

        public DateTime EffectiveDate { get; set; }
    }
}
