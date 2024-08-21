// <copyright file="FieldValidationErrorViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A view model for field validation errors.
    /// </summary>
    public class FieldValidationErrorViewModel : GeneralValidationErrorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValidationErrorViewModel"/> class.
        /// </summary>
        /// <param name="fieldName">The name of the field that failed validation.</param>
        /// <param name="message">The validation message.</param>
        public FieldValidationErrorViewModel(string fieldName, string message)
            : base(message)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(fieldName));

            this.FieldName = fieldName;
        }

        /// <summary>
        /// Gets the name of the field that failed validation.
        /// </summary>
        public string FieldName { get; }
    }
}
