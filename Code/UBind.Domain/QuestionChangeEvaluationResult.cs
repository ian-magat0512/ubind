// <copyright file="QuestionChangeEvaluationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the QuestionChange Evaluation result.
    /// </summary>
    public class QuestionChangeEvaluationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionChangeEvaluationResult"/> class.
        /// </summary>
        /// <param name="changeInvalidatesApproval">The change Invalidates Approval.</param>
        /// <param name="questionChangeResult">The Question Change Result.</param>
        public QuestionChangeEvaluationResult(bool changeInvalidatesApproval, List<QuestionValueChange> questionChangeResult)
        {
            this.ChangeInvalidatesApproval = changeInvalidatesApproval;
            this.QuestionChangeResult = questionChangeResult;
        }

        /// <summary>
        /// Gets a value indicating whether Change invalidates approval.
        /// </summary>
        public bool ChangeInvalidatesApproval { get; }

        /// <summary>
        /// Gets a Question Change Result.
        /// </summary>
        public List<QuestionValueChange> QuestionChangeResult { get; }

        /// <summary>
        /// Returns an object which represents an invalid question change has happened.
        /// </summary>
        /// <param name="questionChangeResult">The Question Change Result.</param>
        /// <returns>Question Change Evaluation Result.</returns>
        public static QuestionChangeEvaluationResult InvalidQuestionChangeEvaluationResult(List<QuestionValueChange> questionChangeResult) => new QuestionChangeEvaluationResult(true, questionChangeResult);

        /// <summary>
        /// Returns an object which represents an evaluation of question changes returned no problems.
        /// </summary>
        /// <returns>Question Change Evaluation Result.</returns>
        public static QuestionChangeEvaluationResult ValidQuestionChangeEvaluationResult() => new QuestionChangeEvaluationResult(false, null);
    }
}
