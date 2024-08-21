// <copyright file="QuestionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the Question Metadta.
    /// </summary>
    public class QuestionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionData"/> class.
        /// </summary>
        /// <param name="questionkey">The Question key.</param>
        /// <param name="value">the value of question.</param>
        /// <param name="index">the index of the repeating question.</param>
        public QuestionData(string questionkey, string value, int? index = null)
        {
            this.Questionkey = questionkey;
            this.Value = value;
            this.Index = index;
        }

        /// <summary>
        /// Gets the questionkey.
        /// </summary>
        public string Questionkey { get; }

        /// <summary>
        /// Gets the value of question.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the value of question.
        /// </summary>
        public int? Index { get; }
    }
}
