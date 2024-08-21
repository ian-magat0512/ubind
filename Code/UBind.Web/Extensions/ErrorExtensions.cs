// <copyright file="ErrorExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using UBind.Domain;
    using UBind.Web.Infrastructure;

    /// <summary>
    /// The error extension.
    /// </summary>
    public static class ErrorExtensions
    {
        /// <summary>
        /// Converts a UBind.Domain.Error to an Microsoft.AspNetCore.Mvc.JsonResult.
        /// </summary>
        /// <param name="error">The Error instance.</param>
        /// <returns>a Microsoft.AspNetCore.Mvc.JsonResult that can be returned by a controller action.</returns>
        public static JsonResult ToProblemJsonResult(this Error error)
        {
            var ubindProblemDetails = UBindProblemDetails.FromError(error);
            return new JsonResult(ubindProblemDetails)
            {
                StatusCode = (int)error.HttpStatusCode,
                ContentType = "application/problem+json; charset=utf-8",
            };
        }
    }
}
