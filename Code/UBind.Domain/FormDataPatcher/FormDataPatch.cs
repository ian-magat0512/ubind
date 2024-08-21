// <copyright file="FormDataPatch.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataPatcher
{
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;

    /// <summary>
    /// The patch for form data.
    /// </summary>
    public class FormDataPatch : IEnumerable<FormDataPatchOperation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataPatch"/> class.
        /// </summary>
        /// <param name="operations">The operations for patch.</param>
        public FormDataPatch(IEnumerable<FormDataPatchOperation> operations)
        {
            this.Operations = new List<FormDataPatchOperation>(operations);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataPatch"/> class.
        /// </summary>
        public FormDataPatch()
        {
            this.Operations = new List<FormDataPatchOperation>();
        }

        private List<FormDataPatchOperation> Operations { get; set; }

        /// <inheritdoc/>
        public IEnumerator<FormDataPatchOperation> GetEnumerator()
        {
            return this.Operations.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Operations.GetEnumerator();
        }

        /// <summary>
        /// Add patch operations to list.
        /// </summary>
        /// <param name="operation"> The operation.</param>
        public void Add(FormDataPatchOperation operation)
        {
            this.Operations.Add(operation);
        }

        /// <summary>
        /// Add patch operations to list.
        /// </summary>
        /// <param name="operation"> The operation.</param>
        public void AddIfNotNull(FormDataPatchOperation operation)
        {
            if (operation?.Value == null)
            {
                return;
            }

            this.Operations.Add(operation);
        }

        /// <summary>
        /// Serialize this object to json.
        /// </summary>
        /// <returns>The json string.</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Create a json patch document.
        /// </summary>
        /// <returns>The json patch document object.</returns>
        public JsonPatchDocument ToJsonPatchDocument()
        {
            return JsonConvert.DeserializeObject<JsonPatchDocument>(this.ToJson());
        }
    }
}
