// <copyright file="AdditionalPropertyValueModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// View model that contains the persisted or mapped values between the additional property definitiona and
    /// the entity it is associated to.
    /// </summary>
    public class AdditionalPropertyValueModel
    {
        [JsonConstructor]
        public AdditionalPropertyValueModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyValueModel"/> class.
        /// </summary>
        /// <param name="dto">Additional property value dto.</param>
        public AdditionalPropertyValueModel(AdditionalPropertyValueDto dto)
        {
            this.Value = dto.Value;
            this.EntityId = dto.EntityId.ToString();
            this.Id = dto?.Id;
            this.AdditionalPropertyDefinitionModel = new AdditionalPropertyDefinitionModel(
                dto.AdditionalPropertyDefinition);
        }

        /// <summary>
        /// Gets or sets the ID of the additional property value.
        /// For text additional property value it is the primary ID of
        /// <see cref="TextAdditionalPropertyValueReadModel"/>.
        /// The Id can be null if no value is set, and we're returning the default value.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the value when this was created.
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value of the ID of an entity associated with this record.
        /// </summary>
        [JsonProperty]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets its parent model.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyDefinitionModel AdditionalPropertyDefinitionModel { get; set; }
    }
}
