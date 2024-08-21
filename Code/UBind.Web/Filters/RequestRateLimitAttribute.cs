// <copyright file="RequestRateLimitAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using StackExchange.Profiling;
    using UBind.Application.Commands.RequestRateLimit;
    using UBind.Application.Configuration;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Configuration;

    [AttributeUsage(AttributeTargets.Method)]
    public class RequestRateLimitAttribute : ActionFilterAttribute
    {
        public int Period { get; set; }

        public RateLimitPeriodType Type { get; set; }

        public int Limit { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (MiniProfiler.Current.Step("Checking rate limit"))
            {
                if (context.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    return;
                }

                var headerConfiguration = context.HttpContext.RequestServices
                    .GetService(typeof(ICustomHeaderConfiguration)) as ICustomHeaderConfiguration;
                var rateLimitConfiguration = context.HttpContext.RequestServices
                    .GetService(typeof(IRateLimitConfiguration)) as IRateLimitConfiguration;
                var mediator = context.HttpContext.RequestServices
                    .GetService(typeof(ICqrsMediator)) as ICqrsMediator;
                var endpoint = $"{context.HttpContext.Request.Method.ToLowerInvariant()}:{context.HttpContext.Request.Path}";
                var period = this.Period > 0 ? this.Period : rateLimitConfiguration.DefaultPeriod;
                var limit = this.Limit > 0 ? this.Limit : rateLimitConfiguration.DefaultLimit;
                var result = await mediator.Send(new DetermineRequestRateLimitCommand(headerConfiguration.ClientIpCode, endpoint, period, this.Type, limit));

                if (!result.IsBlocked)
                {
                    context.HttpContext.Response.OnStarting(() =>
                    {
                        context.HttpContext.Response.Headers.Add("X-Rate-Limit", this.Limit.ToString());
                        context.HttpContext.Response.Headers.Add("X-Rate-Limit-Remaining", result.Limit.ToString());
                        context.HttpContext.Response.Headers.Add("X-Rate-Limit-Expiration", result.PeriodTimestamp.ToString());
                        return Task.CompletedTask;
                    });
                }
                else
                {
                    context.Result = new ContentResult
                    {
                        Content = string.Format(
                                            rateLimitConfiguration.Content,
                                            this.Limit.ToMessage("attempts"),
                                            this.Period.ToMessage(this.Type.Humanize().ToLower()),
                                            result.RetryAfter.ToMessage("seconds")),
                    };
                    context.HttpContext.Response.ContentType = rateLimitConfiguration.ContentType;
                    context.HttpContext.Response.StatusCode = rateLimitConfiguration.StatusCode;
                }

                await base.OnActionExecutionAsync(context, next);
            }
        }
    }
}
