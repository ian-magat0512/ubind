// <copyright file="GraphRequestNotFoundException.cs" company="uBind">
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
    /// Exception for Http request "not found" errors.
    /// </summary>
    [Serializable]
    public class GraphRequestNotFoundException : GraphException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected GraphRequestNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRequestNotFoundException"/> class.
        /// </summary>
        /// <param name="message">A message describing the problem.</param>
        /// <param name="innerException">The exception that triggered this exception.</param>
        private GraphRequestNotFoundException(string message, FlurlHttpException innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="GraphRequestNotFoundException"/> class.
        /// </summary>
        /// <param name="exception">The underlying exception.</param>
        /// <returns>A task from which the new exception can be obtained.</returns>
        public static async Task<GraphRequestNotFoundException> Create(FlurlHttpException exception)
        {
            var url = exception.Call.Request.Url;
            var statusCode = exception.Call.HttpResponseMessage.StatusCode;
            var error = await exception.GetResponseStringAsync();
            var message = $"Request to {url} failed with {statusCode} response error: {error}.";
            return new GraphRequestNotFoundException(message, exception);
        }
    }
}
