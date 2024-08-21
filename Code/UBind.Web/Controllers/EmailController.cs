// <copyright file="EmailController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Email;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for email requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/email")]
    [RequiresFeature(Feature.MessageManagement)]
    public class EmailController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IEmailQueryService emailQueryService;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailController"/> class.
        /// </summary>
        /// <param name="emailQueryService">The email service to retrieve email records.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public EmailController(
            IEmailQueryService emailQueryService,
            IAuthorisationService authorisationService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.emailQueryService = emailQueryService;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Retrieves emails registered in the database.
        /// </summary>
        /// <param name="options">The additional query options to be used.</param>
        /// <returns>List of emails.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewMessages, Permission.ViewAllMessages)]
        [ProducesResponseType(typeof(IEnumerable<EmailSetModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmails([FromQuery] EmailQueryOptionsRequestModel options)
        {
            var tenantId = this.User.GetTenantId();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var filters = await options.ToFilters(
                tenantId,
                this.cachingResolver,
                $"{nameof(Email)}.{nameof(Email.CreatedTicksSinceEpoch)}");
            await this.authorisationService.ApplyViewMessageRestrictionsToFilters(this.User, filters);
            var query = new GetEmailSummariesByFilterQuery(tenantId, filters);
            var emails = await this.mediator.Send(query);
            var models = emails.Select(e => new EmailSetModel(e));
            return this.Ok(models);
        }

        /// <summary>
        /// Gets the details of a specific email record based on the ID given.
        /// </summary>
        /// <param name="emailId">The ID of the email the caller is requesting.</param>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [Route("{emailId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewMessages, Permission.ViewAllMessages)]
        [ProducesResponseType(typeof(EmailDetailSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmailDetails(Guid emailId, [FromQuery] string tenant)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var emailDetails = this.emailQueryService.GetDetails(tenantModel.Id, emailId);
            if (emailDetails == null)
            {
                return Errors.General.NotFound("email", emailId).ToProblemJsonResult();
            }

            await this.authorisationService.ThrowIfUserCannotViewEmail(this.User, emailDetails);
            EmailDetailSetModel detailModel = new EmailDetailSetModel(emailDetails);
            return this.Ok(detailModel);
        }

        /// <summary>
        /// Retrieves the attachment.
        /// </summary>
        /// <param name="emailId">The ID of the email the caller is requesting.</param>
        /// <param name="attachmentId">The attachment ID.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [Route("{emailId}/attachment/{attachmentId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewMessages, Permission.ViewAllMessages)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public IActionResult GetEmailAttachment(Guid emailId, Guid attachmentId)
        {
            var fileContent = this.emailQueryService
                .GetEmailAttachment(this.User.GetTenantId(), emailId, attachmentId);

            if (fileContent == null)
            {
                return Errors.General.NotFound("attachment", attachmentId).ToProblemJsonResult();
            }

            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }
    }
}
