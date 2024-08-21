// <copyright file="IFormDataSchema.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;
    using UBind.Domain.Json;

    /// <summary>
    /// Repository for user profile pictures.
    /// </summary>
    public interface IFormDataSchema
    {
        /// <summary>
        /// Gets a value indicating whether Change invalidate approval.
        /// </summary>
        /// <param name="previousFormData">The Previous Form data json.</param>
        /// <param name="newFormData">The New Form data json.</param>
        /// <returns>boolean whether Change invalidate approval.</returns>
        QuestionChangeEvaluationResult EvaluateQuestionChanges(
            CachingJObjectWrapper previousFormData,
            CachingJObjectWrapper newFormData);

        /// <summary>
        /// Get the qeustions metadata from the form schema.
        /// </summary>
        /// <returns>return the list of Questions.</returns>
        IReadOnlyList<IQuestionMetaData> GetQuestionMetaData();
    }
}
