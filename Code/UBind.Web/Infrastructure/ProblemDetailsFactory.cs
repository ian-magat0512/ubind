// <copyright file="ProblemDetailsFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Infrastructure
{
    using System;
    using Hellang.Middleware.ProblemDetails;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Web.Exceptions;

    /// <summary>
    /// Service for mapping exceptions to instances of <see cref="ProblemDetails"></see> for use in error responses.
    /// </summary>
    public static class ProblemDetailsFactory
    {
        /// <summary>
        /// Register all mapping functions with the problem details middleware.
        /// </summary>
        /// <param name="options">Options for configuring the problem details middleware.</param>
        public static void RegisterMappings(ProblemDetailsOptions options)
        {
            options.Map<ErrorException>(MapErrorException);
            options.Map<NotAuthorisedException<Guid>>(MapNotAuthorisedException<Guid>);
            options.Map<NotAuthorisedException<string>>(MapNotAuthorisedException<string>);
            options.Map<UnauthorizedException>(MapUnauthorizedException);
            options.Map<NotFoundException>(MapNotFoundException);
            options.Map<BadRequestException>(MapBadRequestException);
            options.Map<ProductConfigurationException>(MapProductConfigurationException);
            options.Map<JsonSerializationException>(MapJsonSerializationException);
            options.Map<OperationCanceledException>(MapOperationCancelledException);
            options.MapStatusCode = ProblemDetailsFactory.MapStatusCode;
        }

        private static UBindProblemDetails MapJsonSerializationException(Exception ex)
        {
            if (ex.InnerException != null && ex.InnerException is ErrorException)
            {
                return MapErrorException(ex.InnerException as ErrorException);
            }
            else
            {
                return MapErrorException(new ErrorException(Errors.General.Unexpected(ex.Message)));
            }
        }

        private static UBindProblemDetails MapErrorException(ErrorException ex) => UBindProblemDetails.FromError(ex.Error);

        private static UBindProblemDetails MapNotAuthorisedException<T>(NotAuthorisedException<T> ex) =>
            CreateNotAuthorisedDetails(ex.Message);

        private static UBindProblemDetails MapUnauthorizedException(UnauthorizedException ex) =>
            CreateNotAuthorisedDetails(ex.Message);

        private static UBindProblemDetails MapNotFoundException(NotFoundException ex) =>
           new UBindProblemDetails
           {
               Type = "Not-Found",
               Title = "Not found",
               Status = StatusCodes.Status404NotFound,
               Detail = ex.Message,
           };

        private static UBindProblemDetails MapOperationCancelledException(OperationCanceledException ex) =>
            UBindProblemDetails.FromError(Errors.General.OperationCancelled());

        private static UBindProblemDetails MapBadRequestException(BadRequestException ex) =>
            new UBindProblemDetails
            {
                Type = "Bad-Request",
                Title = "Bad request",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
            };

        private static UBindProblemDetails MapProductConfigurationException(ProductConfigurationException ex) =>
            new UBindProblemDetails
            {
                Type = "Product-Configuration-Error",
                Title = "Product configuration error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
            };

        private static UBindProblemDetails CreateNotAuthorisedDetails(string detail) =>
            new UBindProblemDetails
            {
                Type = "Not-Authorized",
                Title = "Not authorised",
                Status = StatusCodes.Status403Forbidden,
                Detail = detail,
            };

        private static UBindProblemDetails MapStatusCode(HttpContext httpContext)
        {
            var statusCode = httpContext.Response.StatusCode;
            switch (statusCode)
            {
                case 403:
                    return UBindProblemDetails.FromError(Errors.General.NotAuthorized());
                case 401:
                    return UBindProblemDetails.FromError(Errors.General.NotAuthenticated());
                default:
                    var details = new StatusCodeProblemDetails(statusCode);
                    return new UBindProblemDetails
                    {
                        Type = details.Type,
                        Title = details.Title,
                        Status = details.Status,
                        Detail = details.Detail,
                    };
            }
        }
    }
}
