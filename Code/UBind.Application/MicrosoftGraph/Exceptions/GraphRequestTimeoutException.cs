// <copyright file="GraphRequestTimeoutException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using Flurl.Http;

    /// <summary>
    /// Exception for Http request timeouts.
    /// </summary>
    [Serializable]
    public class GraphRequestTimeoutException : GraphException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestTimeoutException"/> class.
        /// </summary>
        /// <param name="exception">The exception that triggered this exception.</param>
        public GraphRequestTimeoutException(FlurlHttpException exception)
            : base($"Request to {exception.Call.Request.Url} timed out after {exception.Call.Duration.Value.TotalMilliseconds} milliseconds.", exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestTimeoutException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected GraphRequestTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
