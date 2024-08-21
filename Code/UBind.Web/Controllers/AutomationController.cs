// <copyright file="AutomationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Configuration;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.Infrastructure;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Handles automation requests.
    /// </summary>
    [Route("/api/v1/tenant/{tenant}/product/{product}/environment/{environment}/automation/{endPoint}/{*pathParameters}")]
    [Route("/api/v1/tenant/{tenant}/organisation/{organisation}/product/{product}/environment/{environment}/automation/{endPoint}/{*pathParameters}")]
    [Route("/api/v1/tenant/{tenant}/product/{product}/environment/{environment}/product-release/{productRelease}/automation/{endPoint}/{*pathParameters}")]
    [Route("/api/v1/tenant/{tenant}/organisation/{organisation}/product/{product}/environment/{environment}/product-release/{productRelease}/automation/{endPoint}/{*pathParameters}")]
    [Route("/api/v1/tenant/{tenant}/product/{product}/environment/{environment}/automations/{endPoint}/{*pathParameters}")] // obsolete
    [Route("/api/v1/tenant/{tenant}/organisation/{organisation}/product/{product}/environment/{environment}/automations/{endPoint}/{*pathParameters}")] // obsolete
    [Route("/api/v1/tenants/{tenant}/products/{product}/environment/{environment}/automations/{endPoint}/{*pathParameters}")] // obsolete
    [Route("/api/v1/tenants/{tenant}/organisation/{organisation}/products/{product}/environment/{environment}/automations/{endPoint}/{*pathParameters}")] // obsolete
    public class AutomationController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IOrganisationService organisationService;
        private readonly IAutomationService automationService;
        private readonly ICustomHeaderConfiguration customHeaderConfiguration;
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationController"/> class.
        /// </summary>
        /// <param name="automationService">The automation service.</param>
        /// <param name="customHeaderConfiguration">The configuration for custom header used to track remote IP address.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public AutomationController(
            IAutomationService automationService,
            ICustomHeaderConfiguration customHeaderConfiguration,
            IOrganisationService organisationService,
            ICachingResolver cachingResolver,
            IMediator mediator)
        {
            this.organisationService = organisationService;
            this.cachingResolver = cachingResolver;
            this.automationService = automationService;
            this.customHeaderConfiguration = customHeaderConfiguration;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handles an automation request.
        /// </summary>
        /// <param name="tenant">The ID or Alias of the tenant.</param>
        /// <param name="organisation">The ID or Alias of the organisation.</param>
        /// <param name="product">The ID or Alias of the product.</param>
        /// <param name="environment">The environment the request is for.</param>
        /// <param name="cancellationToken">For internal use. The cancellation token is automatically injected by the
        /// server. When an automation request is cancelled (e.g. by the browser), this token is activated, allowing
        /// the system to stop processing the request, and therfore not waste any further resources.
        /// </param>
        /// <returns>Ok.</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 180)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
        /* Initially set it to read only, and if it's read-write then we'll create a new scope for it to execute in */
        [RequestIntent(Domain.Patterns.Cqrs.RequestIntent.ReadOnly)]
        public async Task<IActionResult> PerformRequest(
            string tenant,
            string? organisation,
            string product,
            string environment,
            string? productRelease,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // manually cancel after server timeout.
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(99));

            using (MiniProfiler.Current.Step(nameof(AutomationController) + "." + nameof(this.PerformRequest)))
            {
                var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
                var organisationId = string.IsNullOrEmpty(organisation)
                    ? (await this.cachingResolver.GetTenantOrThrow(productModel.TenantId)).Details.DefaultOrganisationId
                    : (await this.cachingResolver.GetOrganisationOrThrow(productModel.TenantId, new GuidOrAlias(organisation))).Id;
                var model = new AutomationRequest(this.ControllerContext.HttpContext);
                var triggerRequest = await model.ToTriggerRequest(this.customHeaderConfiguration.ClientIpCode);
                if (!Enum.TryParse(environment, true, out DeploymentEnvironment environmentResult))
                {
                    throw new ErrorException(Errors.Automation.EnvironmentNotFound(environment));
                }
                this.ThrowIfRequestUsesObsoletePathSegments(environmentResult, triggerRequest);

                var productReleaseId = productRelease != null
                    ? await this.cachingResolver.GetProductReleaseIdOrThrow(
                        productModel.TenantId,
                        productModel.Id,
                        new GuidOrAlias(productRelease))
                    : await this.mediator.Send(
                        new GetDefaultProductReleaseIdOrThrowQuery(
                            productModel.TenantId,
                            productModel.Id,
                            environmentResult),
                        cancellationToken);
                var releaseContext = new ReleaseContext(
                    productModel.TenantId,
                    productModel.Id,
                    environmentResult,
                    productReleaseId);

                try
                {
                    var automationData = await this.automationService.TriggerHttpAutomation(
                    releaseContext, organisationId, triggerRequest, cts.Token);

                    if (automationData.Error != null)
                    {
                        var error = UBindProblemDetails.FromError(automationData.Error);
                        return new JsonResult(error)
                        {
                            StatusCode = (int?)automationData.Error.HttpStatusCode,
                            ContentType = "application/problem+json; charset=utf-8",
                        };
                    }

                    var httpTrigger = automationData.Trigger as HttpTriggerData;
                    var httpResponse = httpTrigger.HttpResponse;
                    var httpContent = await HttpContentBuilder.Build(
                        httpResponse.Content,
                        httpResponse.ContentType,
                        httpResponse.CharacterSet,
                        automationData);

                    foreach (var key in httpResponse.Headers)
                    {
                        this.Response.Headers.Add(key.Key, new StringValues(key.Value.ToArray()));
                    }

                    if (httpResponse.HttpStatusCode != 0)
                    {
                        this.Response.StatusCode = (int)httpResponse.HttpStatusCode;
                    }

                    if (httpResponse.ReasonPhrase != null)
                    {
                        this.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = httpResponse.ReasonPhrase;
                    }

                    // We must check for StringContent first because StringContent extends ByteArrayContent
                    if (httpContent is StringContent)
                    {
                        return await this.GetStringContentResult(httpContent, httpResponse);
                    }
                    else if (httpContent is ByteArrayContent)
                    {
                        var content = await httpContent.ReadAsByteArrayAsync();
                        return new FileContentResult(content, httpResponse.ContentType);
                    }
                    else
                    {
                        // We don't yet have support for other Content (e.g. MulitPartContent) so for now we'll treat
                        // the rest as StringContent and return a ContentResult.
                        return httpContent != null
                            ? await this.GetStringContentResult(httpContent, httpResponse)
                            : new NoContentResult();
                    }
                }
                catch (OperationCanceledException)
                {
                    // We don't yet have support for other Content (e.g. MulitPartContent) so for now we'll treat
                    // the rest as StringContent and return a ContentResult.
                    throw new ErrorException(Errors.General.RequestTimedOut());
                }
            }
        }

        private async Task<ContentResult> GetStringContentResult(HttpContent httpContent, Response httpResponse)
        {
            var content = await httpContent.ReadAsStringAsync();
            return new ContentResult
            {
                ContentType = httpResponse.ContentType,
                Content = content,
            };
        }

        private void ThrowIfRequestUsesObsoletePathSegments(DeploymentEnvironment environment, TriggerRequest triggerRequest)
        {
            if (environment == DeploymentEnvironment.Production)
            {
                // in the production environment we continue to support the obsolete path segments
                return;
            }

            if (triggerRequest.PathSegments[2] == "tenants")
            {
                throw new ErrorException(Errors.Automation.HttpRequest.PathSegmentObsolete(
                    "tenants",
                    "tenant",
                    new List<string> { $"Endpoint URL: {triggerRequest.Url.ToString()}" }));
            }

            if (triggerRequest.PathSegments[4] == "products" || triggerRequest.PathSegments[6] == "products")
            {
                throw new ErrorException(Errors.Automation.HttpRequest.PathSegmentObsolete(
                    "products",
                    "product",
                    new List<string> { $"Endpoint URL: {triggerRequest.Url.ToString()}" }));
            }

            if (triggerRequest.PathSegments[8] == "automations"
                || (triggerRequest.PathSegments.Length > 10 && triggerRequest.PathSegments[10] == "automations"))
            {
                throw new ErrorException(Errors.Automation.HttpRequest.PathSegmentObsolete(
                    "automations",
                    "automation",
                    new List<string> { $"Endpoint URL: {triggerRequest.Url.ToString()}" }));
            }
        }
    }
}
