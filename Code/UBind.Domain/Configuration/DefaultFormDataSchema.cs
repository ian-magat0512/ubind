// <copyright file="DefaultFormDataSchema.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Configuration
{
    using System.Collections.Generic;
    using UBind.Domain.Json;

    /// <summary>
    /// Gets the Default Form DataSchema.
    /// </summary>
    internal class DefaultFormDataSchema : IFormDataSchema
    {
        private static DefaultFormDataSchema instance;

        /// <summary>
        /// Gets the singleton instance of the default Form data schema.
        /// </summary>
        public static IFormDataSchema Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultFormDataSchema();
                }

                return instance;
            }
        }

        /// <inheritdoc/>
        public QuestionChangeEvaluationResult EvaluateQuestionChanges(
            CachingJObjectWrapper previousFormData,
            CachingJObjectWrapper newFormData)
        {
            return QuestionChangeEvaluationResult.ValidQuestionChangeEvaluationResult();
        }

        /// <inheritdoc/>
        public IReadOnlyList<IQuestionMetaData> GetQuestionMetaData()
        {
            return new List<IQuestionMetaData>();
        }
    }
}
