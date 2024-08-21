// <copyright file="AccessLogMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware;

using Serilog;
using UBind.Application.Configuration;
using UBind.Application.ExtensionMethods;

/// <summary>
/// Ensures that all requests are logged to the access log
/// </summary>
public class AccessLogMiddleware
{
    private readonly RequestDelegate next;

    public AccessLogMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICustomHeaderConfiguration headerConfiguration)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await this.next(context);
        }
        finally
        {
            var userId = context.User.GetId()?.ToString() ?? "-";
            var headerCode = headerConfiguration.ClientIpCode;
            var clientIpAddress = context.GetClientIPAddress(headerCode)?.ToString();
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.ForContext("LogType", "AccessLog")
                .ForContext("RemoteIpAddress", clientIpAddress ?? "-")
                .ForContext("UserId", userId)
                .ForContext("HttpRequestProtocol", context.Request.Protocol)
                .ForContext("Referrer", context.Request.Headers["Referer"].FirstOrDefault() ?? "-")
                .ForContext("UserAgent", context.Request.Headers["User-Agent"].FirstOrDefault() ?? "-")
                .ForContext("ResponseLength", context.Response.ContentLength ?? 0)
                .ForContext("Duration", duration)
                .Information(
                    "Request {Method} {Path} responded {StatusCode} in {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration);
        }
    }
}
