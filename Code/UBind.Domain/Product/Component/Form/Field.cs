// <copyright file="Field.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// A field within a form.
    /// </summary>
    public abstract class Field : INamed
    {
        private string key;

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Property")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key for this Field, which is typically a camelCase version of the name.
        /// </summary>
        [WorkbookTableColumnName("Key")]
        public string Key
        {
            get
            {
                if (this.key == null)
                {
                    return this.Name.ToCamelCase();
                }

                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Gets or sets the QuestionSet this field is within.
        /// </summary>
        [Required]
        [JsonIgnore]
        public QuestionSet QuestionSet { get; set; }

        /// <summary>
        /// Gets the key of the field's question set.
        /// </summary>
        public string QuestionSetKey => this.QuestionSet.Key;

        /// <summary>
        /// Gets or sets the data type of the field.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Data Type")]
        [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
        public DataType DataType { get; set; }

        /// <summary>
        /// Gets or sets an expression which if evaluates to true, will cause the field to not be rendered.
        /// </summary>
        [WorkbookTableColumnName("Hide Condition")]
        [WorkbookTableColumnName("Hide Condition Expression")]
        public string HideConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the role or purpose this field's value will use for during the lifecycle
        /// of the form.
        /// </summary>
        [WorkbookTableColumnName("Workflow Role")]
        [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
        public WorkflowRole WorkflowRole { get; set; }

        /// <summary>
        /// Gets or sets the tags to associate with the field.
        /// Tags can be used for a number of purposes, including:
        /// - generating summary tables of the fields with a given tag
        /// - categorising data.
        /// </summary>
        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the currency code for amounts stored by this field.
        /// If the field is limited to a specific currency code, this property can hold that
        /// code.
        /// This field is only applicable if the DataType is Currency, and is optional.
        /// If it's not set, the default currency code for the product is used.
        /// </summary>
        public string CurrencyCode { get; set; }
    }
}
