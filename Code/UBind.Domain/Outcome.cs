// <copyright file="Outcome.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the outcome of an request to an external system that can succeed, fail, or have an error.
    /// </summary>
    public enum Outcome
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// The request was successful.
        /// </summary>
        Success = 1,

        /// <summary>
        /// UBind was able to make request and process the response, but the request did not succeed.
        /// The user could try again with different parameters etc.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// There was an error while making the request.
        /// This is not a problem is not with the request, but due to system misconfiguration, network outages etc.
        /// </summary>
        Error = 3,
    }
}
