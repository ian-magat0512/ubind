// <copyright file="AddressController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.DataDownloader;
    using UBind.Application.Queries.ThirdPartyDataSets;
    using UBind.Application.Queries.ThirdPartyDataSets.Gnaf;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.GnafUpdaterJob;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// The controller for handling vehicle-related queries and requests.
    /// </summary>
    [Route("/api/v1/gnaf")]
    public class AddressController : Controller
    {
        /// <summary>
        /// The mediator service.
        /// </summary>
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator service.</param>
        public AddressController(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Search Gnaf address by Id.
        /// </summary>
        /// <param name="gnafAddressId">The Gnaf address Id.</param>
        /// <returns>Return the Gnaf address.</returns>
        [HttpGet]
        [Route("address/{gnafAddressId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(typeof(AddressResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddressById(string gnafAddressId)
        {
            var gnafMaterializedAddress = await this.mediator.Send(new AddressSearchByAddressIdQuery(gnafAddressId));

            if (gnafMaterializedAddress != null)
            {
                return this.Ok(new AddressResourceModel(gnafMaterializedAddress));
            }

            return this.NotFound();
        }

        /// <summary>
        /// Search address from Gnaf database.
        /// </summary>
        /// <param name="search">The search string.</param>
        /// <param name="maxResults">The max number of results.</param>
        /// <returns>List of Gnaf addresses.</returns>
        [HttpGet]
        [Route("address")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(typeof(AddressSearchResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddressBySearchStringAndMaxResult(string search, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Passing the cancellation token to the mediator's Send method
            var gnafMaterializedAddresses = await this.mediator.Send(new AddressSearchBySearchStringQuery(search, maxResults), cancellationToken);
            var result = new AddressSearchResponseModel
            {
                Addresses = gnafMaterializedAddresses.Select(address => new AddressResourceModel(address)),
            };

            return this.Ok(result);
        }

        /// <summary>
        /// Download a new Gnaf address dataset to be used for address searches.
        /// </summary>
        /// <param name="downloadUrl">The download URL. Please select the latest dataset from https://data.gov.au/data/dataset/geocoded-national-address-file-g-naf in zip format.</param>
        /// <param name="force">When set to false (the default) it will not re-download files which have already been downloaded and have a hash that matches. Set this to true to force it to download the file.</param>
        /// <returns>Action result ok/error.</returns>
        [HttpPost("update-job")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(UpdaterJobStatusResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDataset(string downloadUrl, bool force = false)
        {
            var downloadUrls = new (string Url, string FileHash, string fileName)[] { (downloadUrl, string.Empty, string.Empty) };
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Http, downloadUrls, force);

            var createUpdateJobStatus = await this.mediator.Send(new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest));
            return this.Ok(createUpdateJobStatus);
        }

        /// <summary>
        /// Get Gnaf list updater job records.
        /// </summary>
        /// <returns>List of update tracking records.</returns>
        [HttpGet("update-job")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(IEnumerable<JobStatusResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetListUpdaterJobs()
        {
            var listOfjobStatus = await this.mediator.Send(new GetUpdaterJobsStatusesQuery(typeof(UpdaterJobStateMachine)));

            return this.Ok(listOfjobStatus);
        }

        /// <summary>
        /// Get Gnaf update job record.
        /// </summary>
        /// <param name="updateJobId">The updater job id.</param>
        /// <returns>List of update history.</returns>
        [HttpGet("update-job/{updateJobId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpdateJob(Guid updateJobId)
        {
            var jobStatus = await this.mediator.Send(new GetUpdaterJobStatusQuery(typeof(UpdaterJobStateMachine), updateJobId));
            return this.Ok(jobStatus);
        }

        /// <summary>
        /// Cancel the Update Job (ie set it's status to Cancelled).
        /// </summary>
        /// <param name="updateJobId">the UUID of the update job.</param>
        /// <returns>An updater jobs status.</returns>
        [HttpDelete("update-job/{updateJobId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateJobStatus([FromRoute] Guid updateJobId)
        {
            var jobStatus = await this.mediator.Send(new CancelUpdaterJobCommand(typeof(UpdaterJobStateMachine), updateJobId));
            return this.Ok(jobStatus);
        }
    }
}
