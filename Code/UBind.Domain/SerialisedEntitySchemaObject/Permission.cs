// <copyright file="Permission.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed because we need to generate json representation of permission object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Permission : ISchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Permission"/> class.
        /// </summary>
        /// <param name="type">The permission type.</param>
        public Permission(string type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets the permission type.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
