// <copyright file="CompositeValidationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Validation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Holds the validation result when validating an object or list, which can result it multiple validation
    /// results an object can have multiple properties.
    /// </summary>
    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> results = new List<ValidationResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeValidationResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="subjectType">The type of the object which had the validation error.</param>
        /// <param name="subjectName">The name of the object instance which had the validation error.</param>
        public CompositeValidationResult(string errorMessage, Type subjectType, string subjectName = null)
            : base(errorMessage)
        {
            this.SubjectType = subjectType;
            this.SubjectName = subjectName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeValidationResult"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="memberNames">The member names.</param>
        /// <param name="subjectType">The type of the object which had the validation error.</param>
        /// <param name="subjectName">The name of the object instance which had the validation error.</param>
        public CompositeValidationResult(
            string errorMessage,
            IEnumerable<string> memberNames,
            Type subjectType,
            string subjectName = null)
            : base(errorMessage, memberNames)
        {
            this.SubjectType = subjectType;
            this.SubjectName = subjectName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeValidationResult"/> class.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        protected CompositeValidationResult(ValidationResult validationResult)
            : base(validationResult)
        {
        }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        public IEnumerable<ValidationResult> Results
        {
            get
            {
                return this.results;
            }
        }

        /// <summary>
        /// Gets the type which the validation errors were generated against.
        /// </summary>
        public Type SubjectType { get; }

        /// <summary>
        /// Gets the name of the subject, to help in debugging.
        /// </summary>
        public string SubjectName { get; }

        /// <summary>
        /// Adds a validation result to the list.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        public void AddResult(ValidationResult validationResult)
        {
            this.results.Add(validationResult);
        }
    }
}
