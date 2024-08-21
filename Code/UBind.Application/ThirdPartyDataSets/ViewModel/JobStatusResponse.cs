// <copyright file="JobStatusResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.ViewModel
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc/>
    public class JobStatusResponse : IJobStatusResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobStatusResponse"/> class.
        /// </summary>
        /// <param name="stateMachineJobs">The state machine jobs.</param>
        public JobStatusResponse(StateMachineJob stateMachineJobs)
        {
            this.Id = stateMachineJobs.Id;
            this.HangfireJobId = stateMachineJobs.HangfireJobId;
            this.Step = stateMachineJobs.State.ToCamelCase();
            this.StartDateTime = stateMachineJobs.StartTimestamp.ToString();
            this.StartTicksSinceEpoch = stateMachineJobs.StartTicksSinceEpoch;
            this.CreatedDateTime = stateMachineJobs.CreatedTimestamp.ToString();
            this.CreatedTicksSinceEpoch = stateMachineJobs.CreatedTicksSinceEpoch;
            this.SerializedError = stateMachineJobs.SerializedError;
            this.IsDownloaded = stateMachineJobs.IsDownloaded;
            this.IsExtracted = stateMachineJobs.IsExtracted;
            this.DatasetUrl = stateMachineJobs.DatasetUrl;
            this.EndDateTime = stateMachineJobs.EndTimestamp.ToString();
            this.EndTicksSinceEpoch = stateMachineJobs.EndTicksSinceEpoch;
        }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "hangfireJobId")]
        public string HangfireJobId { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "datasetUrl")]
        public string DatasetUrl { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "step")]
        public string Step { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "startDateTime")]
        public string StartDateTime { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "startTicksSinceEpoch")]
        public long? StartTicksSinceEpoch { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "endDateTime")]
        public string EndDateTime { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "endTicksSinceEpoch")]
        public long? EndTicksSinceEpoch { get; }

        /// <inheritdoc/>
        [JsonIgnore]
        public string SerializedError { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isDownloaded")]
        public bool IsDownloaded { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "isExtracted")]
        public bool IsExtracted { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "error")]
        public Error Error => string.IsNullOrEmpty(this.SerializedError) ? null : JsonConvert.DeserializeObject<Error>(this.SerializedError);

        /// <inheritdoc/>
        public Guid Id { get; }

        [JsonProperty(PropertyName = "createdDateTime")]
        public string CreatedDateTime { get; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "createdTicksSinceEpoch")]
        public long? CreatedTicksSinceEpoch { get; }
    }
}
