// <copyright file="QuestionValueChange.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents a question's value which has changed.
    /// This is used for storing the detected changes between two different points in time for question values.
    /// This is typically used when we are trying to see if something has changed in the answers.
    /// after a quote has been approved. Since most questions for an approved quote cannot be changed, we need a
    /// way to store and represent the changed question values.
    /// </summary>
    public class QuestionValueChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionValueChange"/> class.
        /// </summary>
        /// <param name="questionkey">The Question key.</param>
        /// <param name="previousValue">the previous value of question.</param>
        /// <param name="newValue">the new value of question.</param>
        /// <param name="valueChanged">indicator if question value changed.</param>
        /// <param name="changeAllowedAfterApproval">indicates whether the change is allowed after the quote has been approved.</param>
        public QuestionValueChange(string questionkey, string previousValue, string newValue, bool valueChanged, bool changeAllowedAfterApproval)
        {
            this.Questionkey = questionkey;
            this.PreviousValue = previousValue;
            this.NewValue = newValue;
            this.ValueChanged = valueChanged;
            this.ChangeAllowedAfterApproval = changeAllowedAfterApproval;
        }

        /// <summary>
        /// Gets the questionkey.
        /// </summary>
        public string Questionkey { get; }

        /// <summary>
        /// Gets the previous question value.
        /// </summary>
        public string PreviousValue { get; }

        /// <summary>
        /// Gets the value of question.
        /// </summary>
        public string NewValue { get; }

        /// <summary>
        /// Gets a value indicating whether the question value changed.
        /// </summary>
        public bool ValueChanged { get; }

        /// <summary>
        /// Gets a value indicating whether a change to this question's value is allowed after the quote has been approved.
        /// </summary>
        public bool ChangeAllowedAfterApproval { get; }
    }
}
