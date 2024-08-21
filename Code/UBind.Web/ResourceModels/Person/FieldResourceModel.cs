// <copyright file="FieldResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// The common properties of the field resource model.
    /// </summary>
    public abstract class FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResourceModel"/> class.
        /// </summary>
        /// <param name="id">The id of the record.</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="customLabel">The custom label of the field.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="isDefault">Indicates whether the field is the default or not.</param>
        public FieldResourceModel(
            Guid id,
            string label,
            string customLabel,
            int sequenceNumber = 0,
            bool isDefault = false)
        {
            this.Id = id;
            this.Label = label;
            this.CustomLabel = customLabel;
            this.SequenceNo = sequenceNumber;
            this.IsDefault = isDefault;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResourceModel"/> class.
        /// </summary>
        protected FieldResourceModel()
        {
        }

        /// <summary>
        /// Gets or sets the field ID.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the label of the field.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

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
    }
}
