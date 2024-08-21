// <copyright file="StateMachineJob.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// IDE0060: Removed unused parameter.
// disable IDE0060 because there are unused parameter that cannot be removed.
#pragma warning disable IDE0060

namespace UBind.Domain.ThirdPartyDataSets
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// The state machine job entity to be use in scheduled job.
    /// </summary>
    [Table("StateMachineJob", Schema = "JobScheduler")]
    public class StateMachineJob : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineJob"/> class.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="createdTimestamp">The created time of state machine job.</param>
        /// <param name="stateMachineJobType">The state machine job type. eg. Redbook, GNAF, Glass's Guide, etc.</param>
        /// <param name="hangfireJobId">The hangfire job id.</param>
        /// <param name="stateMachineJobManifest">The serialized state machine job manifest.</param>
        public StateMachineJob(Guid id, Instant createdTimestamp, string stateMachineJobType, string hangfireJobId, string datasetUrl, string stateMachineJobManifest)
            : base(id, createdTimestamp)
        {
            this.StateMachineJobType = stateMachineJobType;
            this.HangfireJobId = hangfireJobId;
            this.StartTimestamp = createdTimestamp;
            this.StateMachineJobManifest = stateMachineJobManifest;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineJob"/> class.
        /// A protected, parameterless constructor for EF, allowing proxy generation for lazy loading.
        /// </summary>
        protected StateMachineJob()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets or sets the Hangfire job id .
        /// </summary>
        public string HangfireJobId { get; set; }

        /// <summary>
        /// Gets or sets the state machine job type .
        /// </summary>
        public string StateMachineJobType { get; set; }

        /// <summary>
        /// Gets or sets the State .
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the start time of the job.
        /// </summary>
        public Instant? StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets start time as the number of ticks since the Unix epoch.
        /// </summary>
        public long? StartTicksSinceEpoch
        {
            get => this.StartTimestamp?.ToUnixTimeTicks();
            set => this.StartTimestamp = value.HasValue ? Instant.FromUnixTimeTicks(value.Value) : (Instant?)null;
        }

        /// <summary>
        /// Gets or sets the end time of the job.
        /// </summary>
        public Instant? EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the end time as the number of ticks since the Unix epoch.
        /// </summary>
        public long? EndTicksSinceEpoch
        {
            get => this.EndTimestamp?.ToUnixTimeTicks();
            set => this.EndTimestamp = value.HasValue ? Instant.FromUnixTimeTicks(value.Value) : (Instant?)null;
        }

        /// <summary>
        /// Gets or sets the Serialized error object of the job.
        /// </summary>
        public string SerializedError { get; set; }

        /// <summary>
        /// Gets the Deserialized error object of of the job.
        /// </summary>
        public Error Error => string.IsNullOrEmpty(this.SerializedError) ? null : JsonConvert.DeserializeObject<Error>(this.SerializedError);

        /// <summary>
        /// Gets or sets state machine job manifest.
        /// </summary>
        public string StateMachineJobManifest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data set is downloaded.
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the downloaded data set is extracted .
        /// </summary>
        public bool IsExtracted { get; set; }

        /// <summary>
        /// Gets or sets the data set download url.
        /// </summary>
        public string DatasetUrl { get; set; }

        /// <summary>
        /// Update the state of the state machine.
        /// </summary>
        /// <param name="state">The new state.</param>
        public void SetState(string state)
        {
            this.State = state;
        }

        /// <summary>
        /// Update the serialized error of the state machine job.
        /// </summary>
        /// <param name="serializedError">The serialized error.</param>
        public void UpdateSerializedError(string serializedError)
        {
            this.SerializedError = serializedError;
        }

        /// <summary>
        /// Set end time of the state machine job.
        /// </summary>
        /// <param name="endTimestamp">The end time.</param>
        public void SetEndTime(Instant endTimestamp)
        {
            this.EndTimestamp = endTimestamp;
        }
    }
}
