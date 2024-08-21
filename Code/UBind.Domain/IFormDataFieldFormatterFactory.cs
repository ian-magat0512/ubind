// <copyright file="IFormDataFieldFormatterFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using CSharpFunctionalExtensions;
    using UBind.Domain.FormDataFieldFormatters;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// A class which creates and holds IFormDataFieldFormatter instances.
    /// </summary>
    public interface IFormDataFieldFormatterFactory
    {
        /// <summary>
        /// Creates a new instance of an IFormDataFieldFormatter for the given data type.
        /// This is use for formatting fields in a standard way for the given data type.
        /// </summary>
        /// <param name="dataType">The dataType.</param>
        /// <returns>An instance of formdata field wrapped in a Result.</returns>
        Result<IFormDataFieldFormatter> Create(DataType dataType);
    }
}
