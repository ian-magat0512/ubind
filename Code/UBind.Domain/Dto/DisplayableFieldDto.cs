// <copyright file="DisplayableFieldDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto
{
    using System.Collections.Generic;

    /// <summary>
    /// Data transfer object for Displayable Field.
    /// </summary>
    public class DisplayableFieldDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayableFieldDto"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for JSON deserializer.</remarks>
        public DisplayableFieldDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayableFieldDto"/> class.
        /// </summary>
        /// <param name="displayableFields">The displayable fields.</param>
        /// <param name="repeatingDisplayableFields">The repeating displayable fields.</param>
        /// <param name="displayableFieldsEnabled">The status whether displayable field is enabled.</param>
        /// <param name="repeatingDisplayableFieldsEnabled">The status whether repeating displayable field is enabled.</param>
        public DisplayableFieldDto(List<string> displayableFields, List<string> repeatingDisplayableFields, bool displayableFieldsEnabled, bool repeatingDisplayableFieldsEnabled)
        {
            this.DisplayableFields = displayableFields;
            this.RepeatingDisplayableFields = repeatingDisplayableFields;
            this.DisplayableFieldsEnabled = displayableFieldsEnabled;
            this.RepeatingDisplayableFieldsEnabled = repeatingDisplayableFieldsEnabled;
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
