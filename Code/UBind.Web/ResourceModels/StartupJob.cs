// <copyright file="StartupJob.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;

    public class StartupJob
    {
        public StartupJob(Domain.StartupJob startupJob)
        {
            this.Id = startupJob.Id;
            this.Alias = startupJob.Alias;
            this.Started = startupJob.Started;
            this.Complete = startupJob.Complete;
            this.Blocking = startupJob.Blocking;
            this.RunManuallyInMultiNodeEnvironment = startupJob.RunManuallyInMultiNodeEnvironment;
            this.HangfireJobId = startupJob.HangfireJobId;
            this.CreatedDateTime = startupJob.CreatedTimestamp.ToString();
        }

        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the alias of the job.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the startup job has begun executing.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the job processing is completed.
        /// </summary>
        public bool Complete { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the startup application will wait for the job
        /// to complete before making the application available.
        /// </summary>
        public bool Blocking { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the startup job will be run manually
        /// when being triggered on a multinode environment.
        /// Default value is false.
        /// </summary>
        public bool RunManuallyInMultiNodeEnvironment { get; private set; }

        /// <summary>
        /// Gets the hangfire job ID for this startup job, if one has been created, otherwise null.
        /// </summary>
        public string HangfireJobId { get; private set; }

        /// <summary>
        /// Gets an ISO 8601 string describing the date and time at which this startup job was created.
        /// </summary>
        public string CreatedDateTime { get; private set; }
    }
}
