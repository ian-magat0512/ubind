// <copyright file="WebServiceIntegrationResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Net;

    /// <summary>
    /// Response for third-party web service integrations.
    /// </summary>
    public class WebServiceIntegrationResponse
    {
        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        public string ResponseJson { get; set; }

        /// <summary>
        /// Gets or sets the response error message, if any.
        /// </summary>
        public string ErrorJson { get; set; }
    }
}
