// <copyright file="ImportData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports.MappingObjects
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents an array of data based from import.
    /// </summary>
    public class ImportData
    {
        /// <summary>
        /// Gets the array of data from the given payload.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public JArray Data { get; private set; }
    }
}
