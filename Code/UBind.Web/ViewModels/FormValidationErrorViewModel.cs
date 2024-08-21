// <copyright file="FormValidationErrorViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// A view model for form validation errors.
    /// </summary>
    public class FormValidationErrorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormValidationErrorViewModel"/> class.
        /// </summary>
        /// <param name="generalErrors">Form-level validation errros.</param>
        /// <param name="fieldErrors">Form field validation errors.</param>
        public FormValidationErrorViewModel(
            IEnumerable<GeneralValidationErrorViewModel> generalErrors,
            IEnumerable<FieldValidationErrorViewModel> fieldErrors)
        {
            Contract.Assert(generalErrors != null);
            Contract.Assert(fieldErrors != null);

            this.GeneralErrors = generalErrors.ToList();
            this.FieldErrors = fieldErrors.ToList();
        }

        /// <summary>
        /// Gets form-level validation errors.
        /// </summary>
        public IEnumerable<GeneralValidationErrorViewModel> GeneralErrors { get; }

        /// <summary>
        /// Gets form field validation errors.
        /// </summary>
        public IEnumerable<FieldValidationErrorViewModel> FieldErrors { get; }
    }
}
