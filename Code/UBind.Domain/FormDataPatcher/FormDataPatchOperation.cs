// <copyright file="FormDataPatchOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataPatcher
{
    using Newtonsoft.Json;

    /// <summary>
    /// Referring to a patch operation for form data.
    /// </summary>
    public abstract class FormDataPatchOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataPatchOperation"/> class.
        /// </summary>
        /// <param name="operation">The patch operation type.</param>
        /// <param name="path">The target path for patch operation.</param>
        /// <param name="value">The value for patch operation.</param>
        public FormDataPatchOperation(string operation, string path, string value)
        {
            this.Op = operation;
            this.Path = path;
            this.Value = value;
        }

        /// <summary>
        /// Gets the operation of the patch.
        /// </summary>
        [JsonProperty(PropertyName = "op")]
        public string Op { get; }

        /// <summary>
        /// Gets the target path to be patched.
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path { get; }

        /// <summary>
        /// Gets the value to be patched.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; }
    }
}
