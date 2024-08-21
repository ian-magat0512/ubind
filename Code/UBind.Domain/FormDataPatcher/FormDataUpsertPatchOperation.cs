// <copyright file="FormDataUpsertPatchOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataPatcher
{
    /// <summary>
    /// A upsert patch operation for form data.
    /// </summary>
    public class FormDataUpsertPatchOperation : FormDataPatchOperation
    {
        // add operation will create the property if not exists at sets the value for an existing property.
        private const string Operation = "add";

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataUpsertPatchOperation"/> class.
        /// </summary>
        /// <param name="path">The target path.</param>
        /// <param name="value">The value to be upsert.</param>
        public FormDataUpsertPatchOperation(string path, string value)
            : base(Operation, path, value)
        {
        }
    }
}
