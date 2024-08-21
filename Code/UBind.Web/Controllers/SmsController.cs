// <copyright file="SmsController.cs" company="uBind">
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
    using UBind.Application.Queries.Sms;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.ReadWriteModel;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for message requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/sms")]
    public class SmsController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsController"/> class.
        /// </summary>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public SmsController(
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the messages from the databases.
        /// </summary>
        /// <param name="options">The additional query options to be used.</param>
        /// <returns>List of messages.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewMessages, Permission.ViewAllMessages)]
        [ProducesResponseType(typeof(IEnumerable<Sms>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] SmsQueryOptionsModel options)
        {
            var tenantId = this.User.GetTenantId();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var filters = await options.ToFilters(tenantId, this.cachingResolver, $"{nameof(Sms)}.{nameof(Sms.CreatedTicksSinceEpoch)}");
            await this.authorisationService.ApplyViewMessageRestrictionsToFilters(this.User, filters);
            var query = new GetAllSmsByFilterQuery(tenantId, filters);
            var sms = await this.mediator.Send(query);
            var models = sms.Select(s => new SmsSetModel(s));
            return this.Ok(models);
        }

        /// <summary>
        /// Get the sms by id.
        /// </summary>
        /// <param name="smsId">The sms id.</param>
        /// <returns>The sms dto.</returns>
        [HttpGet]
        [Route("{smsId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewMessages, Permission.ViewAllMessages)]
        [ProducesResponseType(typeof(ISmsDetails), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid smsId)
        {
            var query = new GetSmsDetailsByIdQuery(this.User.GetTenantId(), smsId);
            var smsDetails = await this.mediator.Send(query);
            return this.Ok(new SmsDetailSetModel(smsDetails));
        }
    }
}
