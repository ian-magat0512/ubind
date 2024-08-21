// <copyright file="StartupJobController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.StartupJobs;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller responsible for handling requests related to startup-jobs.
    /// </summary>
    [Route("api/v1/startup-job/")]
    public class StartupJobController : Controller
    {
        private readonly IStartupJobRunner startupJobRunner;
        private readonly IStartupJobRepository startupJobRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupJobController"/> class.
        /// </summary>
        /// <param name="startupJobRunner">The startup job runner.</param>
        /// <param name="startupJobRepository">The startup job repository.</param>
        public StartupJobController(IStartupJobRunner startupJobRunner, IStartupJobRepository startupJobRepository)
        {
            this.startupJobRunner = startupJobRunner;
            this.startupJobRepository = startupJobRepository;
        }

        /// <summary>
        /// Handles requests for triggering startup-jobs with the given name.
        /// </summary>
        /// <param name="startupJobName">The alias of the startup job.</param>
        /// <param name="force">Run the startup job even if it's already started or completed.</param>
        /// <returns>A result.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [MustHavePermission(Permission.ManageStartupJobs)]
        [Route("{startupJobName}/run")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult RunJob(string startupJobName, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(startupJobName))
            {
                throw new ErrorException(Errors.StartupJob.MethodNameIsRequired());
            }

            var startupJob = this.startupJobRepository.GetStartupJobByAlias(startupJobName);
            if (startupJob == null)
            {
                throw new ErrorException(Errors.StartupJob.MethodNameNotFound(startupJobName));
            }

            if (!force && startupJob.Complete)
            {
                throw new ErrorException(Errors.StartupJob.StartupJobIsAlreadyComplete(startupJobName));
            }

            if (!force && !startupJob.Blocking && startupJob.Started)
            {
                throw new ErrorException(Errors.StartupJob.StartupJobHasAlreadyStarted(startupJobName, startupJob.HangfireJobId));
            }

            this.startupJobRunner.ThrowIfPrecedingStartupJobsHaveNotBeenCompleted(startupJob);
            this.startupJobRunner.EnqueueStartupJob(startupJobName, force);
            return this.Ok("Job execution triggered.");
        }

        /// <summary>
        /// Lists the incomplete startup jobs.
        /// </summary>
        /// <returns>The incomplete startup jobs.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [MustHavePermission(Permission.ManageStartupJobs)]
        [Route("incomplete")]
        [ProducesResponseType(typeof(IEnumerable<ResourceModels.StartupJob>), StatusCodes.Status200OK)]
        public JsonResult ListIncompleteJobs()
        {
            var jobRecords = this.startupJobRepository.GetIncompleteStartupJobs()
                .Select(jr => new ResourceModels.StartupJob(jr));
            return this.Json(jobRecords);
        }

        /// <summary>
        /// Gets the status of a startup job.
        /// This is intended to be publicly accessible and does not require authentication.
        /// </summary>
        /// <returns>Status of startup job.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [Route("{startupJobAlias}/status")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetStartupJobStatus(string startupJobAlias)
        {
            var startupJob = this.startupJobRepository.GetStartupJobByAlias(startupJobAlias);
            if (startupJob == null)
            {
                throw new ErrorException(Errors.StartupJob.StartupJobNotFound(startupJobAlias));
            }
            return this.Ok(startupJob.Status.Humanize());
        }

        /// <summary>
        /// Notifies staff of the incomplete startup jobs by email.
        /// </summary>
        /// <returns>The incomplete startup jobs.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageStartupJobs)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [Route("notify-incomplete")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult NotifyIncompleteJobs()
        {
            this.startupJobRunner.NotifyIncompleteJobs();
            return this.Ok("An email will be sent if there are any incomplete startup jobs.");
        }
    }
}
