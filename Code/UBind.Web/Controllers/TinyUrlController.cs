// <copyright file="TinyUrlController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBind.Application.Queries.TinyUrl;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Web.Filters;

/// <summary>
/// Handles redirection of tiny URLs
/// </summary>
[Route("/t")]
public class TinyUrlController : Controller
{
    private readonly ICqrsMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TinyUrlController"/> class.
    /// </summary>
    public TinyUrlController(
        ICqrsMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Redirects to the url associated with the token.
    /// </summary>
    [HttpGet]
    [Route("{token}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
    public async Task<IActionResult> RedirectFromToken(string token)
    {
        var url = this.HttpContext.Request.GetFullUrl();
        if (string.IsNullOrEmpty(token))
        {
            return this.NotFound(Errors.RedirectUrl.NotFound(url));
        }

        var redirectUrl = await this.mediator.Send(new GetRedirectUrlQuery(token));
        if (string.IsNullOrEmpty(redirectUrl))
        {
            return this.NotFound(Errors.RedirectUrl.NotFound(url));
        }

        return this.RedirectPermanent(redirectUrl);
    }
}
