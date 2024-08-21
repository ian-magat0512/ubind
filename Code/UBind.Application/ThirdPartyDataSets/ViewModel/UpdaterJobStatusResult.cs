// <copyright file="UpdaterJobStatusResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.ViewModel
{
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc/>
    public class UpdaterJobStatusResult : IUpdaterJobStatusResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobStatusResult"/> class.
        /// </summary>
        /// <param name="stateMachineJobs">The state machine jobs.</param>
        public UpdaterJobStatusResult(StateMachineJob stateMachineJobs)
        {
            var jobStatus = new JobStatusResponse(stateMachineJobs);
            this.JobStatusResult = new JobStatusResult(Result.Success<JobStatusResponse, Error>(jobStatus));

            this.Id = stateMachineJobs.Id.ToString();
            this.CreationOptions = "none";
        }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "result")]
        public JobStatusResult JobStatusResult { get; private set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "exception")]
        public Error Error { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isCanceled")]
        public bool IsCanceled { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isCompleted")]
        public bool IsCompleted { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "creationOptions")]
        public string CreationOptions { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "asyncState")]
        public string AsyncState { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isFaulted")]
        public bool IsFaulted { get; }

        /// <inheritdoc/>
        public void SetJobStatusResult(Result<JobStatusResponse, Error> jobStatusResult)
        {
            this.JobStatusResult = new JobStatusResult(jobStatusResult);
        }
    }
}
