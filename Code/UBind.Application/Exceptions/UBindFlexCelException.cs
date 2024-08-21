// <copyright file="UBindFlexCelException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Raised when there is an error using the flexcell workbook.
    /// </summary>
    /// <remarks>
    /// It is considered a product configuration error if the workbook cannot be used as expected.
    /// </remarks>
    [Serializable]
    public class UBindFlexCelException : ProductConfigurationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UBindFlexCelException"/> class.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <param name="innerException">The underlying error.</param>
        public UBindFlexCelException(Error error, Exception innerException)
        : base(error, innerException)
        {
        }
    }
}
