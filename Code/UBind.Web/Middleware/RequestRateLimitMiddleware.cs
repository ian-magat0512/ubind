// <copyright file="RequestRateLimitMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using UBind.Application.Commands.RequestRateLimit;
    using UBind.Application.Configuration;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Configuration;
    using UBind.Web.Filters;

    /// <summary>
    /// Inspects the [RequestRateLimit] attribute on the controller and action, and if present, enforces the rate.
    /// </summary>
    public class RequestRateLimitMiddleware
    {
        private readonly RequestDelegate next;

        public RequestRateLimitMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(
            HttpContext httpContext,
            ICqrsMediator mediator,
            IRateLimitConfiguration rateLimitConfiguration)
        {
            var endpoint = httpContext.GetEndpoint();
            var isValidRequest = await this.ValidateRequestRateLimit(httpContext, endpoint, mediator, rateLimitConfiguration);

            if (!isValidRequest)
            {
                return;
            }

            await this.next(httpContext);
        }

        private async Task<bool> ValidateRequestRateLimit(
            HttpContext httpContext,
            Endpoint endpoint,
            ICqrsMediator mediator,
            IRateLimitConfiguration rateLimitConfiguration)
        {
            if (endpoint == null)
            {
                return true;
            }

            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                var methodInfo = controllerActionDescriptor.MethodInfo;
                if (methodInfo != null)
                {
                    var hasRequestLimitAttr = methodInfo.CustomAttributes.Where(a => a.AttributeType == typeof(RequestRateLimitAttribute)).Any();
                    if (!hasRequestLimitAttr)
                    {
                        var headerConfiguration = httpContext.RequestServices
                            .GetService(typeof(ICustomHeaderConfiguration)) as ICustomHeaderConfiguration;

                        var endpointName = $"{httpContext.Request.Method.ToLowerInvariant()}:{httpContext.Request.Path}";
                        Enum.TryParse(rateLimitConfiguration.DefaultPeriodType, true, out RateLimitPeriodType periodType);

                        var result = await mediator.Send(new DetermineRequestRateLimitCommand(
                            headerConfiguration.ClientIpCode,
                            endpointName,
                            rateLimitConfiguration.DefaultPeriod,
                            periodType,
                            rateLimitConfiguration.DefaultLimit));

                        if (!result.IsBlocked)
                        {
                            httpContext.Response.OnStarting(() =>
                            {
                                httpContext.Response.Headers.Add("X-Rate-Limit", rateLimitConfiguration.DefaultLimit.ToString());
                                httpContext.Response.Headers.Add("X-Rate-Limit-Remaining", result.Limit.ToString());
                                httpContext.Response.Headers.Add("X-Rate-Limit-Expiration", result.PeriodTimestamp.ToString());
                                return Task.CompletedTask;
                            });
                            return true;
                        }
                        else
                        {
                            var message = string.Format(
                                    rateLimitConfiguration.Content,
                                    rateLimitConfiguration.DefaultLimit.ToMessage("attempts"),
                                    rateLimitConfiguration.DefaultPeriod.ToMessage(rateLimitConfiguration.DefaultPeriodType.ToLower()),
                                    result.RetryAfter.ToMessage("seconds"));
                            httpContext.Response.ContentType = rateLimitConfiguration.ContentType;
                            httpContext.Response.StatusCode = rateLimitConfiguration.StatusCode;
                            await httpContext.Response.WriteAsync(message, Encoding.UTF8);
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
