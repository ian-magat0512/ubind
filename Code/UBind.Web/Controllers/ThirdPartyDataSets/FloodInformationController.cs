// <copyright file="FloodInformationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.ThirdPartyDataSets
{
    using System;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using StackExchange.Profiling;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.DataDownloader;
    using UBind.Application.Queries.ThirdPartyDataSets;
    using UBind.Application.Queries.ThirdPartyDataSets.Nfid;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.NfidUpdaterJob;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;

    /// <summary>
    /// The controller for handling nfid queries and requests.
    /// </summary>
    [Route("/api/v1/nfid")]
    public class FloodInformationController : Controller
    {
        private readonly IMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloodInformationController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public FloodInformationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Retrieves the list of updater jobs.
        /// </summary>
        /// <returns>Action result ok/error.</returns>
        [HttpGet("update-job")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        public async Task<IActionResult> GetListUpdaterJobs()
        {
            var listOfjobStatus = await this.mediator.Send(new GetUpdaterJobsStatusesQuery(typeof(UpdaterJobStateMachine)));
            return this.Json(listOfjobStatus);
        }

        /// <summary>
        /// Get NFID details by gnaf address id.
        /// </summary>
        /// <param name="gnafAddressId">The gnaf address id.</param>
        /// <returns>An updater jobs status.</returns>
        [HttpGet("{gnafAddressId}")]
        public async Task<IActionResult> GetNfidStage6ByGnafIdQuery(string gnafAddressId)
        {
            using (MiniProfiler.Current.Step(nameof(FloodInformationController) + "." + nameof(this.GetNfidStage6ByGnafIdQuery)))
            {
                var result = await this.mediator.Send(new GetStage6ByGnafIdQuery(gnafAddressId));
                return this.Json(result);
            }
        }

        /// <summary>
        /// Get the updater job status.
        /// </summary>
        /// <param name="updateJobId">The updater job id.</param>
        /// <returns>An updater jobs status.</returns>
        [HttpGet("update-job/{updateJobId}")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        public async Task<IActionResult> GetUpdaterJobsStatus(Guid updateJobId)
        {
            var jobStatus = await this.mediator.Send(new GetUpdaterJobStatusQuery(typeof(UpdaterJobStateMachine), updateJobId));
            return this.Json(jobStatus);
        }

        /// <summary>
        /// Create or Update NFID updater job.
        /// </summary>
        /// <param name="downloadUrl">The download url of the NFID dataset.</param>
        /// <param name="force">To force to overwrite the DataSet.</param>
        /// <returns>Action result ok/error.</returns>
        [HttpPost("update-job")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        public async Task<IActionResult> CreateUpdateJob(string downloadUrl, bool force = false)
        {
            var downloadUrls = new (string Url, string FileHash, string fileName)[] { (downloadUrl, string.Empty, string.Empty) };
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, force);

            var createUpdateJobStatus = await this.mediator.Send(new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest));
            if (createUpdateJobStatus.JobStatusResult.IsSuccess)
            {
                return this.Json(createUpdateJobStatus.JobStatusResult.Value);
            }
            else
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.CannotCreateUpdaterJob());
            }
        }

        /// <summary>
        /// Cancel the Update Job (ie set it's status to Cancelled).
        /// </summary>
        /// <param name="updateJobId">the UUID of the update job.</param>
        /// <returns>An updater jobs status.</returns>
        [HttpDelete("update-job/{updateJobId}")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        public async Task<IActionResult> PatchUpdaterJobRequestVm([FromRoute] Guid updateJobId)
        {
            var jobStatus = await this.mediator.Send(new CancelUpdaterJobCommand(typeof(UpdaterJobStateMachine), updateJobId));
            return this.Json(jobStatus);
        }
    }
}
