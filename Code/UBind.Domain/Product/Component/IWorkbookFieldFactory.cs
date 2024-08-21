// <copyright file="IWorkbookFieldFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component
{
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Factory for instantiating Fields from the type specified in the uBind workbook.
    /// </summary>
    public interface IWorkbookFieldFactory
    {
        /// <summary>
        /// Creates a Field from the given workbook field type string.
        /// </summary>
        /// <param name="workbookFieldType">The field type value as specified in a standard uBind workbook.</param>
        /// <returns>The Field instance.</returns>
        Field Create(string workbookFieldType);
    }
}
