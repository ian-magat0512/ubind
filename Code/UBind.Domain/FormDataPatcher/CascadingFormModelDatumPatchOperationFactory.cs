// <copyright file="CascadingFormModelDatumPatchOperationFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataPatcher
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using UBind.Domain;

    /// <summary>
    /// Sets a new value to a target form model datum path,
    /// and generates a patch operation for the target path.
    /// </summary>
    public class CascadingFormModelDatumPatchOperationFactory
    {
        private const string FormModelKey = "formModel";
        private readonly string[] quoteDatumPaths;
        private readonly string newValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CascadingFormModelDatumPatchOperationFactory"/> class.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <param name="quoteDatumPaths">The quote datum location paths to use, arranged in order.</param>
        public CascadingFormModelDatumPatchOperationFactory(string newValue, params string[] quoteDatumPaths)
        {
            this.quoteDatumPaths = quoteDatumPaths;
            this.newValue = newValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CascadingFormModelDatumPatchOperationFactory"/> class.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <param name="quoteDatumPaths">The quote datum location paths to use, arranged in order.</param>
        public CascadingFormModelDatumPatchOperationFactory(string newValue, IEnumerable<string> quoteDatumPaths)
            : this(newValue, quoteDatumPaths.ToArray())
        {
        }

        /// <summary>
        /// Add patch operation to form data patch.
        /// </summary>
        /// <param name="questionMetaData">The question metadata.</param>
        /// <returns>The upsert patch operation.</returns>
        public Operation? ToUpsertPatchOperation(IEnumerable<IQuestionMetaData> questionMetaData)
        {
            var quoteDatumPath = this.quoteDatumPaths.FirstOrDefault(lp => !string.IsNullOrEmpty(lp));
            if (!questionMetaData.Any(md => md.Key == quoteDatumPath))
            {
                return null;
            }

            return new Operation("add", FormModelKey + "/" + quoteDatumPath, null, this.newValue);
        }
    }
}
