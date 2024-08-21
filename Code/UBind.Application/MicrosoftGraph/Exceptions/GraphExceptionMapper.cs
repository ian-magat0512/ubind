// <copyright file="GraphExceptionMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph.Exceptions
{
    using System.Net;
    using System.Threading.Tasks;
    using Flurl.Http;

    /// <summary>
    /// Helper for wrapping Flurl Http exceptions with specific Graph exceptions.
    /// </summary>
    public static class GraphExceptionMapper
    {
        /// <summary>
        /// Create a new instance of <see cref="GraphException"/> wrapping a given instance of <see cref="FlurlHttpException"/>.
        /// </summary>
        /// <param name="exception">The exception to wrap.</param>
        /// <returns>a new instance of <see cref="GraphException"/> wrapping the given instance of <see cref="FlurlHttpException"/>.</returns>
        public static async Task<GraphException> Wrap(FlurlHttpException exception)
        {
            if (exception.Call.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return await GraphRequestNotFoundException.Create(exception);
            }

            if (exception.InnerException is TaskCanceledException)
            {
                return new GraphRequestTimeoutException(exception);
            }

            return await GraphRequestErrorException.Create(exception);
        }
    }
}
