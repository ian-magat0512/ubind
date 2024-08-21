// <copyright file="Tag.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the base model for all ordered fields for use of automations.
    /// </summary>
    public class Tag : ISchemaObject
    {
        public Tag(UBind.Domain.ReadWriteModel.Tag model)
        {
            this.EntityId = model.EntityId.ToString();
            this.Value = model.Value;
            this.EntityType = model.EntityType.ToString();
            this.TagType = model.TagType.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        [JsonConstructor]
        private Tag()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the entity this tag is for.
        /// </summary>
        [JsonProperty(PropertyName = "entityId", Order = 25)]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type associated with this record.
        /// </summary>
        [JsonProperty(PropertyName = "entityType", Order = 26)]
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the entity tag so its easily searchable associated with this record.
        /// </summary>
        [JsonProperty(PropertyName = "tagtype", Order = 27)]
        public string TagType { get; set; }

        /// <summary>
        /// Gets or sets the entity tag value so its easily searchable associated with this record.
        /// </summary>
        [JsonProperty(PropertyName = "value", Order = 28)]
        public string Value { get; set; }
    }
}
