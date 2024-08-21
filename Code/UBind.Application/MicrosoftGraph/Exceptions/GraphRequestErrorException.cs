// <copyright file="GraphRequestErrorException.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Flurl.Http;

    /// <summary>
    /// Exception for Http request timeouts.
    /// </summary>
    [Serializable]
    public class GraphRequestErrorException : GraphException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestErrorException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected GraphRequestErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestErrorException"/> class.
        /// </summary>
        /// <param name="message">A message describing the problem.</param>
        /// <param name="innerException">The exception that triggered this exception.</param>
        private GraphRequestErrorException(string message, FlurlHttpException innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="GraphRequestErrorException"/> class.
        /// </summary>
        /// <param name="exception">The underlying exception.</param>
        /// <returns>A task from which the new exception can be obtained.</returns>
        public static async Task<GraphRequestErrorException> Create(FlurlHttpException exception)
        {
            var url = exception.Call.Request.Url;
            var error = await exception.GetResponseStringAsync();
            var message = $"Request to {url} failed with error: {error}.";
            return new GraphRequestErrorException(message, exception);
        }
    }
}
