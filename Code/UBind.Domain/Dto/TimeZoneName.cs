// <copyright file="TimeZoneName.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    /// <summary>
    /// This class is needed because we need a data transfer object for time zone name and alias.
    /// </summary>
    public class TimeZoneName
    {
        /// <summary>
        /// Gets or sets the zone name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the zone alias.
        /// </summary>
        public string Alias { get; set; }
    }
}
