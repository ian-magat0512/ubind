// <copyright file="FileContentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using CSharpFunctionalExtensions;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Claim;
    using UBind.Application.Queries.Claim.Version;
    using UBind.Application.Queries.FileAttachment;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services;
    using UBind.Application.Services.PolicyDataPatcher;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for handling file content related requests.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1")]
    [ApiController]
    public class FileContentController : ControllerBase
    {
        private readonly IApplicationQuoteFileAttachmentService quoteApplicationFileAttachmentService;
        private readonly ICqrsMediator mediator;
        private readonly IAuthorisationService authorisationService;
        private readonly IQuoteService quoteService;
        private readonly IPolicyService policyService;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IPatchService patchService;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentController"/> class.
        /// </summary>
        /// <param name="quoteApplicationFileAttachmentService">The quote application file attachment service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="policyService">The policy service.</param>
        /// <param name="cachingResolver">The resolver to get cached copies of tenants and products.</param>
        public FileContentController(
            IApplicationQuoteFileAttachmentService quoteApplicationFileAttachmentService,
            IAuthorisationService authorisationService,
            IQuoteService quoteService,
            ICqrsMediator mediator,
            IPolicyService policyService,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IQuoteAggregateRepository quoteAggregateRepository,
            IPatchService patchService,
            ICachingResolver cachingResolver)
        {
            this.quoteApplicationFileAttachmentService = quoteApplicationFileAttachmentService;
            this.authorisationService = authorisationService;
            this.quoteService = quoteService;
            this.mediator = mediator;
            this.policyService = policyService;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.patchService = patchService;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Gets the file attachment content of a quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="attachmentId">The ID of the file attachment to retrieve.</param>
        /// <param name="quoteAccessToken">An access token for the quote, which allows access to the attachment without being logged in.</param>
        /// <param name="tenant">The tenant ID or alias. If not passed, and the user is logged in, it will assume the user's tenant.</param>
        /// <returns>The quote attachment content.</returns>
        [HttpGet]
        [Route("quote/{quoteId}/attachment/{attachmentId}/content")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteAttachmentContent(
            Guid quoteId, string attachmentId, [FromQuery] string quoteAccessToken, [FromQuery] string tenant)
        {
            this.authorisationService.ThrowIfUserNotLoggedInAndTenantNotSpecified(this.User, tenant);
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));

            // If no valid quote access token is passed, we need to do standard permission checks for logged in users
            // TODO: We have no system for generating quote access tokens, and so no method for properly checking them
            // This will need to be implemented very soon in UB-5419.
            bool validAccessTokenProvided = true;
            if (!validAccessTokenProvided)
            {
                await this.authorisationService.ThrowIfUserCannotViewQuote(this.User, quoteId);
            }

            if (!Guid.TryParse(attachmentId, out Guid id))
            {
                return await this.GetQuoteAttachmentByFileName(tenantModel.Id, quoteId, attachmentId);
            }

            var fileContent = this.quoteApplicationFileAttachmentService.GetFileAttachmentContent(tenantModel.Id, id);
            return this.GetFileContentResult(fileContent);
        }

        /// <summary>
        /// Gets the file attachment content of a quote.
        /// </summary>
        /// <param name="quoteVersionId">The ID of the quote version.</param>
        /// <param name="attachmentId">The ID of the file attachment to retrieve.</param>
        /// <param name="quoteAccessToken">An access token for the quote, which allows access to the attachment without being logged in.</param>
        /// <param name="tenant">The tenant ID or alias. If not passed, and the user is logged in, it will assume the user's tenant.</param>
        /// <returns>The attachment content.</returns>
        [HttpGet]
        [Route("quote-version/{quoteId}/attachment/{attachmentId}/content")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuoteVersionAttachmentContent(Guid quoteVersionId, string attachmentId, [FromQuery] string quoteAccessToken, [FromQuery] string tenant)
        {
            this.authorisationService.ThrowIfUserNotLoggedInAndTenantNotSpecified(this.User, tenant);
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));

            // If no valid quote access token is passed, we need to do standard permission checks for logged in users
            // TODO: We have no system for generating quote access tokens, and so no method for properly checking them
            // This will need to be implemented very soon in UB-5419.
            bool validAccessTokenProvided = true;
            if (!validAccessTokenProvided)
            {
                await this.authorisationService.ThrowIfUserCannotViewQuoteVersion(this.User, quoteVersionId);
            }

            if (!Guid.TryParse(attachmentId, out Guid id))
            {
                var quoteVersion = await this.mediator.Send(new GetQuoteVersionByIdQuery(tenantModel.Id, quoteVersionId));
                return await this.GetQuoteAttachmentByFileName(tenantModel.Id, quoteVersion.QuoteId, attachmentId);
            }

            var fileContent = await this.mediator.Send(
                new GetFileAttachmentContentByQuoteVersionQuery(tenantModel.Id, id, quoteVersionId));
            return this.GetFileContentResult(fileContent);
        }

        /// <summary>
        /// Gets the file attachment content of a policy.
        /// </summary>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="attachmentId">The ID of the file attachment to retrieve.</param>
        /// <param name="quoteAccessToken">An access token for the quote, which allows access to the attachment without being logged in.</param>
        /// <param name="tenant">The tenant ID or alias. If not passed, and the user is logged in, it will assume the user's tenant.</param>
        /// <returns>The policy attachment content.</returns>
        [HttpGet]
        [Route("policy/{policyId}/attachment/{attachmentId}/content")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPolicyAttachmentContent(Guid policyId, string attachmentId, [FromQuery] string quoteAccessToken, [FromQuery] string tenant)
        {
            this.authorisationService.ThrowIfUserNotLoggedInAndTenantNotSpecified(this.User, tenant);
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var tenantId = tenantModel.Id;

            // If no valid quote access token is passed, we need to do standard permission checks for logged in users
            // TODO: We have no system for generating quote access tokens, and so no method for properly checking them
            // This will need to be implemented very soon in UB-5419.
            bool validAccessTokenProvided = true;
            if (!validAccessTokenProvided)
            {
                await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policyId);
            }

            if (!Guid.TryParse(attachmentId, out Guid id))
            {
                return await this.GetPolicyAttachmentByFileName(tenantId, policyId, attachmentId);
            }

            var fileContent = this.quoteApplicationFileAttachmentService.GetFileAttachmentContent(
                tenantModel.Id, id);
            return this.GetFileContentResult(fileContent);
        }

        /// <summary>
        /// Gets the file attachment content of a claim.
        /// </summary>
        /// <param name="claimId">The claim or claim version ID.</param>
        /// <param name="attachmentId">The ID of the file attachment to retrieve.</param>
        /// <param name="claimAccessToken">An access token for the claim, which allows access to the attachment without being logged in.</param>
        /// <param name="tenant">The tenant ID or alias. If not passed, and the user is logged in, it will assume the user's tenant.</param>
        /// <returns>The quote attachment content.</returns>
        [HttpGet]
        [Route("claim/{claimId}/attachment/{attachmentId}/content")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetClaimAttachmenContent(
            Guid claimId, Guid attachmentId, [FromQuery] string claimAccessToken, [FromQuery] string tenant)
        {
            this.authorisationService.ThrowIfUserNotLoggedInAndTenantNotSpecified(this.User, tenant);
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));

            // If no valid claim access token is passed, we need to do standard permission checks for logged in users
            // TODO: We have no system for generating claim access tokens, and so no method for properly checking them
            // This will need to be implemented very soon in UB-5419.
            bool validAccessTokenProvided = true;
            if (!validAccessTokenProvided)
            {
                await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claimId);
            }

            var fileContent =
                await this.mediator.Send(new GetClaimFileAttachmentContentQuery(tenantModel.Id, attachmentId));
            return this.GetFileContentResult(fileContent);
        }

        /// <summary>
        /// Gets the file attachment content of a claim.
        /// </summary>
        /// <param name="claimVersionId">The claim version ID.</param>
        /// <param name="attachmentId">The ID of the file attachment to retrieve.</param>
        /// <param name="claimAccessToken">An access token for the claim, which allows access to the attachment without being logged in.</param>
        /// <param name="tenant">The tenant ID or alias. If not passed, and the user is logged in, it will assume the user's tenant.</param>
        /// <returns>The claim attachment content.</returns>
        [HttpGet]
        [Route("claim-version/{claimVersionId}/attachment/{attachmentId}/content")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetClaimVersionAttachmenContent(Guid claimVersionId, Guid attachmentId, [FromQuery] string claimAccessToken, [FromQuery] string tenant)
        {
            this.authorisationService.ThrowIfUserNotLoggedInAndTenantNotSpecified(this.User, tenant);
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));

            // If no valid claim access token is passed, we need to do standard permission checks for logged in users
            // TODO: We have no system for generating claim access tokens, and so no method for properly checking them
            // This will need to be implemented very soon in UB-5419.
            bool validAccessTokenProvided = true;
            if (!validAccessTokenProvided)
            {
                await this.authorisationService.ThrowIfUserCannotViewClaimVersion(this.User, claimVersionId);
            }

            var fileContent = await this.mediator.Send(
                new GetClaimVersionFileAttachmentContentQuery(tenantModel.Id, attachmentId));
            return this.GetFileContentResult(fileContent);
        }

        private ActionResult GetFileContentResult(Maybe<IFileContentReadModel> fileContent)
        {
            if (fileContent.HasNoValue)
            {
                return this.NotFound();
            }

            var content = fileContent.GetValueOrDefault();
            return this.GetFileContentResult(content);
        }

        private ActionResult GetFileContentResult(IFileContentReadModel fileContent)
        {
            if (fileContent == null)
            {
                return this.NotFound();
            }

            string contentType = string.IsNullOrEmpty(fileContent.ContentType) ? "application/text" : fileContent.ContentType;
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = HttpUtility.UrlEncode(fileContent.Name),
                Inline = false,
            };

            this.Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

            return new FileContentResult(fileContent.FileContent, contentType);
        }

        private async Task<ActionResult> GetQuoteAttachmentByFileName(Guid tenantId, Guid quoteId, string attachmentId)
        {
            var quote = await this.mediator.Send(new GetQuoteByIdQuery(tenantId, quoteId));
            string fileName = this.GetFilenameFromFormData(attachmentId, quote.LatestFormData);
            var content = this.quoteApplicationFileAttachmentService.GetFileAttachmentContent(tenantId, quote.AggregateId, fileName);

            return this.GetFileContentResult(content);
        }

        private async Task<ActionResult> GetPolicyAttachmentByFileName(Guid tenantId, Guid policyId, string attachmentId)
        {
            var policy = await this.policyService.GetPolicy(tenantId, policyId);
            var fileName = this.GetFilenameFromFormData(attachmentId, policy.LatestFormData);
            var content = this.quoteApplicationFileAttachmentService.GetFileAttachmentContent(tenantId, policyId, fileName);

            return this.GetFileContentResult(content);
        }

        private string GetFilenameFromFormData(string attachmentId, string latestFormData)
        {
            var jObject = JObject.Parse(latestFormData);
            var attachmentInfo = jObject["formModel"]?[attachmentId]?.ToString();
            if (string.IsNullOrEmpty(attachmentInfo))
            {
                throw new ErrorException(Errors.Document.DocumentStoredIncorrectly(attachmentId));
            }

            var fileName = attachmentInfo.Split(':')[0];
            return fileName;
        }
    }
}
