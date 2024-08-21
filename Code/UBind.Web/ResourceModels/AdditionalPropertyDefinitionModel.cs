// <copyright file="AdditionalPropertyDefinitionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;
    using UBind.Web.Validation;

    /// <summary>
    /// View model used in adding additional property.
    /// </summary>
    public class AdditionalPropertyDefinitionModel : IAdditionalPropertyDefinitionDetails
    {
        [JsonConstructor]
        public AdditionalPropertyDefinitionModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionModel"/> class.
        /// </summary>
        /// <param name="additionalPropertyDefinitionReadModel">Read model definition from the database.</param>
        public AdditionalPropertyDefinitionModel(
            AdditionalPropertyDefinitionReadModel additionalPropertyDefinitionReadModel)
        {
            this.Alias = additionalPropertyDefinitionReadModel.Alias;
            this.ContextType = additionalPropertyDefinitionReadModel.ContextType;
            this.EntityType = additionalPropertyDefinitionReadModel.EntityType;
            this.Name = additionalPropertyDefinitionReadModel.Name;
            this.IsRequired = additionalPropertyDefinitionReadModel.IsRequired;
            this.Id = additionalPropertyDefinitionReadModel.Id;
            this.IsUnique = additionalPropertyDefinitionReadModel.IsUnique;
            this.DefaultValue = additionalPropertyDefinitionReadModel.DefaultValue;
            this.ParentContextId = additionalPropertyDefinitionReadModel.ParentContextId;
            this.ContextId = additionalPropertyDefinitionReadModel.ContextId;
            this.Type = additionalPropertyDefinitionReadModel.PropertyType;
            this.SchemaType = additionalPropertyDefinitionReadModel.SchemaType;
            this.CustomSchema = additionalPropertyDefinitionReadModel.CustomSchema;
        }

        public AdditionalPropertyDefinitionModel(
            AdditionalPropertyDefinitionDto additionalPropertyDefinitionDto)
        {
            this.Alias = additionalPropertyDefinitionDto.Alias;
            this.ContextType = additionalPropertyDefinitionDto.ContextType;
            this.ContextId = additionalPropertyDefinitionDto.ContextId;
            this.DefaultValue = additionalPropertyDefinitionDto.DefaultValue;
            this.EntityType = additionalPropertyDefinitionDto.EntityType;
            this.Id = additionalPropertyDefinitionDto.Id;
            this.IsRequired = additionalPropertyDefinitionDto.IsRequired;
            this.IsUnique = additionalPropertyDefinitionDto.IsUnique;
            this.ParentContextId = additionalPropertyDefinitionDto.ParentContextId;
            this.Type = additionalPropertyDefinitionDto.PropertyType;
            this.Name = additionalPropertyDefinitionDto.Name;
            this.SchemaType = additionalPropertyDefinitionDto.SchemaType;
            this.CustomSchema = additionalPropertyDefinitionDto.CustomSchema;
        }

        public AdditionalPropertyDefinitionModel(
            AdditionalPropertyDefinitionCreateOrUpdateModel model)
        {
            this.Id = model.Id;
            this.Alias = model.Alias;
            this.ContextType = model.ContextType;
            this.DefaultValue = model.DefaultValue;
            this.EntityType = model.EntityType;
            this.IsRequired = model.IsRequired;
            this.IsUnique = model.IsUnique;
            this.Name = model.Name;
            this.Type = model.Type;
            this.SchemaType = model.SchemaType;
            this.CustomSchema = model.CustomSchema;
        }

        /// <summary>
        /// Gets or sets the primary value.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        [Required]
        public AdditionalPropertyEntityType EntityType { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        [Required(ErrorMessage = "Property name is required.")]
        [EntityName]
        public string Name { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public string DefaultValue { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        [Required]
        public bool IsRequired { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        [Required]
        public bool IsUnique { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        [Required(ErrorMessage = "Property alias is required.")]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase English letters, digits, and hyphens; and must not start or end with a hyphen.")]
        public string Alias { get; set; }

        /// <inheritdoc/>
        [JsonProperty]
        public Guid? ParentContextId { get; set; }

        /// <summary>
        /// Gets or sets the enum <see cref="AdditionalPropertyDefinitionContextType"/>.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyDefinitionContextType ContextType { get; set; }

        /// <summary>
        /// Gets or sets the context id (product id, tenant id or organisation id).
        /// </summary>
        [JsonProperty]
        public Guid ContextId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the property is set to default value.
        /// </summary>
        public bool SetToDefaultValue
        {
            get
            {
                return !string.IsNullOrEmpty(this.DefaultValue);
            }
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionType Type { get; set; }

        /// <summary>
        /// Gets or sets the context id, for tenant and product, it is the alias, for the organisation it is the id.
        /// </summary>
        public string ContextAliasOrId { get; set; }

        /// <summary>
        /// Gets or sets the parent context id , for organisation and product it is the tenant alias, for tenant it is
        /// null.
        /// </summary>
        public string ParentContextAliasOrId { get; set; }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionSchemaType? SchemaType { get; set; }

        /// <inheritdoc/>
        public string? CustomSchema { get; set; }
    }
}
