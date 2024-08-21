// <copyright file="ContentSecurityPolicyMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware;

using Microsoft.AspNetCore.Http;
using UBind.Application.Authorisation;
using UBind.Domain;

/// <summary>
/// For debugging purposes.
/// </summary>
public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate next;
    private readonly IContentSecurityPolicyServiceFactory contentSecurityPolicyServiceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSecurityPolicyMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate.</param>
    public ContentSecurityPolicyMiddleware(
        RequestDelegate next,
        IContentSecurityPolicyServiceFactory contentSecurityPolicyServiceFactory)
    {
        this.next = next;
        this.contentSecurityPolicyServiceFactory = contentSecurityPolicyServiceFactory;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICachingResolver cachingResolver,
        IAuthorisationService authorisationService)
    {
        context.Response.OnStarting(
        async state =>
        {
            AppType appType;
            var httpContext = (HttpContext)state;
            if (context.Request.Path.StartsWithSegments("/portal"))
            {
                appType = AppType.Portal;
            }
            else if (context.Request.Path.Equals("/index.html"))
            {
                appType = AppType.Forms;
            }
            else
            {
                return;
            }

            var contentSecurityPolicyService = this.contentSecurityPolicyServiceFactory.Create(appType);
            string? policyValue = await contentSecurityPolicyService.GetPolicyHeaderValue(httpContext, cachingResolver, authorisationService);
            if (!string.IsNullOrEmpty(policyValue))
            {
                httpContext.Response.Headers.Remove("Content-Security-Policy");
                httpContext.Response.Headers.Add("Content-Security-Policy", policyValue);
            }
        },
        context);

        await this.next(context);
    }
}