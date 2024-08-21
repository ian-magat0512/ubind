// <copyright file="DataLocation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator
{
    /// <summary>
    /// Class for all data location.
    /// </summary>
    public class DataLocation
    {
        public DataLocation(DataSource source, string path)
        {
            this.Source = source;
            this.Path = path;
        }

        /// <summary>
        /// Gets the source where the data is located. (e.g form data, calculation data).
        /// </summary>
        public DataSource Source { get; private set; }

        /// <summary>
        /// Gets the path in the object where the data is located.
        /// </summary>
        public string Path { get; private set; }
    }
}
