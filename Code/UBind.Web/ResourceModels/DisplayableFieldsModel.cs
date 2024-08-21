// <copyright file="DisplayableFieldsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Dto;

    /// <summary>
    /// Resource model for Displayable fields.
    /// </summary>
    public class DisplayableFieldsModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayableFieldsModel"/> class.
        /// </summary>
        /// <param name="displayableFieldDto">The displayable field Data transfer object.</param>
        public DisplayableFieldsModel(DisplayableFieldDto displayableFieldDto)
        {
            if (displayableFieldDto != null)
            {
                this.DisplayableFields = displayableFieldDto.DisplayableFields;
                this.RepeatingDisplayableFields = displayableFieldDto.RepeatingDisplayableFields;
                this.DisplayableFieldsEnabled = displayableFieldDto.DisplayableFieldsEnabled;
                this.RepeatingDisplayableFieldsEnabled = displayableFieldDto.RepeatingDisplayableFieldsEnabled;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayableFieldsModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        private DisplayableFieldsModel()
        {
        }

        /// <summary>
        /// Gets the Fields.
        /// </summary>
        public List<string> DisplayableFields { get; private set; }

        /// <summary>
        /// Gets the Fields.
        /// </summary>
        public List<string> RepeatingDisplayableFields { get; private set; }

        /// <summary>
        /// Gets a value indicating whether displayable field is enabled.
        /// </summary>
        public bool DisplayableFieldsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether repeating displayable field is enabled.
        /// </summary>
        public bool RepeatingDisplayableFieldsEnabled { get; private set; }
    }
}
