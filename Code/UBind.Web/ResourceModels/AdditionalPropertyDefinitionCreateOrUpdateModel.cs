// <copyright file="AdditionalPropertyDefinitionCreateOrUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBind.Domain.Enums;

    public class AdditionalPropertyDefinitionCreateOrUpdateModel : IValidatableObject
    {
        public string Tenant { get; set; }

        public Guid Id { get; set; }

        public string Alias { get; set; }

        public AdditionalPropertyDefinitionContextType ContextType { get; set; }

        public string ContextId { get; set; }

        public string DefaultValue { get; set; }

        public AdditionalPropertyEntityType EntityType { get; set; }

        public bool IsRequired { get; set; }

        public bool IsUnique { get; set; }

        public string Name { get; set; }

        public string ParentContextId { get; set; }

        public bool SetToDefaultValue { get; set; }

        public AdditionalPropertyDefinitionType Type { get; set; }

        public AdditionalPropertyDefinitionSchemaType? SchemaType { get; set; }

        public string? CustomSchema { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.IsUnique && this.SetToDefaultValue)
            {
                yield return new ValidationResult("An additional property with a default value cannot also be unique");
            }
        }
    }
}
