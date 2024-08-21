// <copyright file="Component.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Configuration;
    using UBind.Domain.Validation;

    /// <summary>
    /// Represents the configuration of a product component.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Gets the version of this structure.
        /// </summary>
        public string Version { get; } = "2.0.0";

        /// <summary>
        /// Gets or sets the configuration for the form for this Product Component.
        /// </summary>
        [Required]
        [ValidateObject]
        public Form.Form Form { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the triggers associated with this Product Component.
        /// </summary>
        [ValidateItems]
        public List<Trigger> Triggers { get; set; }

        /// <summary>
        /// Gets or sets the payment form configuration for this component.
        /// </summary>
        public JObject? PaymentFormConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the product formdatalocator for this component.
        /// </summary>
        public JObject? DataLocators { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component calculates using a standard UBind workbook.
        /// </summary>
        public bool CalculatesUsingStandardWorkbook { get; set; }

        /// <summary>
        /// Gets or sets the context entities configuration for this component.
        /// </summary>
        public ContextEntities? ContextEntities { get; set; }

        /// <summary>
        /// Checks the validity of this component, and returns any errors.
        /// </summary>
        /// <returns>The validation errors.</returns>
        public IReadOnlyList<ValidationResult> Validate()
        {
            var validationContext = new ValidationContext(this);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);
            return validationResults;
        }

        /// <summary>
        /// Within the component are references to other objects. When deserializing from json, these references
        /// are not automatically created. This populates them.
        /// For example, a Field holds a reference to it's Question Set, but during deserializing it only
        /// populates the question set key. This function will populate the QuestionSet property with a real
        /// instance.
        /// </summary>
        public void PopulateReferences()
        {
            // During unit test execution Form could be null
            if (this.Form != null)
            {
                this.Form.PopulateReferences();
            }
        }
    }
}
