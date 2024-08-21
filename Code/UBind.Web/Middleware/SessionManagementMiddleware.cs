// <copyright file="SessionManagementMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Primitives;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Redis;
    using UBind.Domain.Services;
    using UBind.Web.Filters;

    /// <summary>
    /// Middleware for handling custom session management using JWTokens
    /// https://blog.getseq.net/smart-logging-middleware-for-asp-net-core/.
    /// </summary>
    public class SessionManagementMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManagementMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next middleware for chaining.</param>
        public SessionManagementMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Handle incomding web request.
        /// </summary>
        /// <param name="httpContext">The Http Context for the request.</param>
        /// <param name="userSessionService">The service for checking user sessions.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Invoke(
            HttpContext httpContext,
            IUserSessionService userSessionService,
            IErrorNotificationService errorNotificationService)
        {
            try
            {
                if (httpContext == null)
                {
                    throw new ArgumentNullException(nameof(httpContext));
                }

                var endpoint = httpContext.GetEndpoint();
                bool isRouteRegistered = endpoint != null;

                // TODO: Instead check Session as part of AspNetCore authentication middleware and get rid this entire class
                bool hasAuthorizationAttribute = this.RouteHasAuthorizationAttribute(endpoint);

                // Only check session for authenticated users for registered routes that are not ignored
                if (httpContext.User != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
                {
                    UserSessionModel? userSession = await userSessionService.Get(httpContext.User);
                    if (isRouteRegistered && hasAuthorizationAttribute)
                    {
                        if (userSession == null)
                        {
                            throw new ErrorException(Errors.User.SessionNotFound());
                        }

                        if (await userSessionService.IsSessionIdle())
                        {
                            throw new ErrorException(Errors.User.SessionExpiredDueToInactivity());
                        }

                        if (await userSessionService.IsSessionExpired())
                        {
                            throw new ErrorException(Errors.User.SessionExpiredDueToMaximumPeriod());
                        }

                        if (await userSessionService.HasPasswordExpired())
                        {
                            throw new ErrorException(Errors.User.PasswordExpiry.UserPasswordExpired(
                                userSession.UserId, userSession.AccountEmailAddress));
                        }

                        // extend the session
                        if (userSession != null)
                        {
                            userSessionService.ExtendIdleTimeout();
                        }
                    }
                }

                await this.next(httpContext);
            }
            catch (Exception exception)
            {
                this.CaptureSentryException(httpContext, errorNotificationService, exception);
                throw;
            }
        }

        private bool RouteHasAuthorizationAttribute(Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return false;
            }

            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                var methodInfo = controllerActionDescriptor.MethodInfo;
                if (methodInfo != null)
                {
                    return methodInfo.CustomAttributes.Where(a =>
                        a.AttributeType == typeof(MustBeLoggedInAttribute)
                        || a.AttributeType == typeof(MustHavePermissionAttribute)
                        || a.AttributeType == typeof(MustHaveOneOfPermissionsAttribute)).Any();
                }
            }

            return false;
        }

        private void CaptureSentryException(HttpContext httpContext, IErrorNotificationService errorNotificationService, Exception exception)
        {
            RouteData? routeData = httpContext?.GetRouteData();
            var environment = string.Empty;
            if (routeData != null)
            {
                routeData.Values.TryGetValue("environment", out object? routeDataEnvironment);
                if (routeDataEnvironment != null)
                {
                    environment = routeDataEnvironment.ToString();
                }
                else
                {
                    StringValues requestEnvironment = string.Empty;
                    httpContext?.Request.Query.TryGetValue("environment", out requestEnvironment);

                    if (!string.IsNullOrEmpty(requestEnvironment))
                    {
                        environment = requestEnvironment.ToString();
                    }
                }
            }
            else
            {
                StringValues requestEnvironment = string.Empty;
                httpContext?.Request.Query.TryGetValue("environment", out requestEnvironment);

                if (!string.IsNullOrEmpty(requestEnvironment))
                {
                    environment = requestEnvironment.ToString();
                }
            }

            if (!string.IsNullOrEmpty(environment))
            {
                Enum.TryParse(environment, true, out DeploymentEnvironment env);
                errorNotificationService.CaptureSentryException(exception, env);
            }
            else
            {
                errorNotificationService.CaptureSentryException(exception, null);
            }
        }
    }
}
