// <copyright file="RepeatingFieldUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Enums;

    /// <summary>
    /// Resource model for binding repeating field query options.
    /// </summary>
    public class RepeatingFieldUpdateModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatingFieldUpdateModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for model binding.</remarks>
        [JsonConstructor]
        public RepeatingFieldUpdateModel()
        {
        }

        /// <summary>
        /// Gets or sets the field ID.
        /// </summary>
        [JsonProperty]
        public Guid? FieldId { get; set; }

        /// <summary>
        /// Gets or sets the field type.
        /// </summary>
        [JsonProperty]
        public FieldType FieldType { get; set; }
    }
}
