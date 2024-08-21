// <copyright file="GraphErrorResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For parsing MS Graph error responses.
    /// </summary>
    public class GraphErrorResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphErrorResponse"/> class.
        /// </summary>
        /// <remarks>Only used for testing.</remarks>
        /// <param name="error">The error.</param>
        public GraphErrorResponse(GraphError error)
        {
            error.ThrowIfArgumentNull(nameof(error));
            this.Error = error;
        }

        [JsonConstructor]
        private GraphErrorResponse()
        {
        }

        /// <summary>
        /// Gets the error (as per https://github.com/microsoftgraph/microsoft-graph-docs/blob/master/concepts/errors.md#error-resource-type).
        /// </summary>
        [JsonProperty]
        public GraphError Error { get; private set; }
    }
}
