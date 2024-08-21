// <copyright file="LabelledOrderedField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// The common properties of the fields.
    /// </summary>
    public abstract class LabelledOrderedField
    {
        private Guid fieldId;
        private string label;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledOrderedField"/> class.
        /// </summary>
        /// <param name="label">The label of the field.</param>
        /// <param name="customLabel">The custom label of the field.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="isDefault">Indicates whether the field is the default or not.</param>
        public LabelledOrderedField(
            string label,
            string customLabel,
            int sequenceNumber = 0,
            bool isDefault = false)
        {
            this.Label = label;
            this.CustomLabel = customLabel;
            this.SequenceNo = sequenceNumber;
            this.IsDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledOrderedField"/> class.
        /// </summary>
        /// <param name="tenantId">The fields tenant Id.</param>
        /// <param name="id">The fields Id.</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="customLabel">The custom label of the field.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="isDefault">Indicates whether the field is the default or not.</param>
        public LabelledOrderedField(
            Guid tenantId,
            Guid id,
            string label,
            string customLabel,
            int sequenceNumber = 0,
            bool isDefault = false)
        {
            this.TenantId = tenantId;
            this.Id = id;
            this.Label = label;
            this.CustomLabel = customLabel;
            this.SequenceNo = sequenceNumber;
            this.IsDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelledOrderedField"/> class.
        /// </summary>
        protected LabelledOrderedField()
        {
        }

        /// <summary>
        /// Gets or sets the field ID.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the field ID.
        /// This will be removed and obsolete.
        /// </summary>
        public Guid FieldId
        {
            get
            {
                return this.Id == Guid.Empty ? this.fieldId : this.Id;
            }

            set
            {
                this.fieldId = value;
            }
        }

        /// <summary>
        /// Gets or sets tenant Id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the label of the field.
        /// </summary>
        [JsonProperty("label")]
        public string Label
        {
            get
            {
                return this.label;
            }

            set
            {
                if (value == null)
                {
                    this.label = null;
                }
                else
                {
                    if (this.LabelOptions.Any(x => x == value.Trim().ToLower()))
                    {
                        this.label = value.Trim().ToLower();
                    }
                    else
                    {
                        this.label = this.LabelOptions.LastOrDefault()?.Trim().ToLower();
                        this.CustomLabel = value.Trim();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom label of the field.
        /// </summary>
        public string CustomLabel { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the field.
        /// </summary>
        public int SequenceNo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is the default or not.
        /// </summary>
        [JsonProperty("default")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets the label options for the specific field.
        /// </summary>
        [JsonIgnore]
        protected abstract IEnumerable<string> LabelOptions { get; }
    }
}
