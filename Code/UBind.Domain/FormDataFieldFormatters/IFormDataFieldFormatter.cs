// <copyright file="IFormDataFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    /// <summary>
    /// Contract for a class which can format a field value based upon the passed metadata.
    /// </summary>
    public interface IFormDataFieldFormatter
    {
        /// <summary>
        /// Get the prettified value of the from field.
        /// </summary>
        /// <param name="value">The raw value string.</param>
        /// <param name="metaData">The field metadata.</param>
        /// <returns>The prettified string value.</returns>
        string Format(string value, IQuestionMetaData metaData);
    }
}
