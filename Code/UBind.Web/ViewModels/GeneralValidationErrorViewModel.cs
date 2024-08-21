// <copyright file="GeneralValidationErrorViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A view model for validation errors.
    /// </summary>
    public class GeneralValidationErrorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralValidationErrorViewModel"/> class.
        /// </summary>
        /// <param name="message">The validation message.</param>
        public GeneralValidationErrorViewModel(string message)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(message));

            this.Message = message;
        }

        /// <summary>
        /// Gets the validation message.
        /// </summary>
        public string Message { get; }
    }
}
