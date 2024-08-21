// <copyright file="GraphError.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using Newtonsoft.Json;

    /// <summary>
    /// For parsing Microsoft Graph Errrors.
    /// </summary>
    public class GraphError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphError"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        public GraphError(string code)
        {
            this.Code = code;
        }

        [JsonConstructor]
        private GraphError()
        {
        }

        /// <summary>
        /// Gets the Error code (as per https://github.com/microsoftgraph/microsoft-graph-docs/blob/master/concepts/errors.md#code-property).
        /// </summary>
        [JsonProperty]
        public string Code { get; private set; }

        /// <summary>
        /// Gets an error message for developer use.
        /// </summary>
        [JsonProperty]
        public string Message { get; private set; }

        /// <summary>
        /// Gets diagnostic error info, where available.
        /// </summary>
        [JsonProperty]
        public dynamic InnerError { get; private set; }
    }
}
