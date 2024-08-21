// <copyright file="RepeatingQuestionSet.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// A question set that is to be repeated multiple times.
    /// </summary>
    public class RepeatingQuestionSet : QuestionSet
    {
        /// <summary>
        /// Gets or sets the key of the repeating field that is configured to repeat this question set.
        /// </summary>
        public string RepeatingFieldKey { get; set; }
    }
}
