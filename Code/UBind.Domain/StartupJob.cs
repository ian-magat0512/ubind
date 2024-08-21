// <copyright file="StartupJob.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// Stores the startup job information.
    /// </summary>
    public class StartupJob : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupJob"/> class.
        /// </summary>
        /// <param name="alias">The alias of the job.</param>
        /// <param name="blocking">The job is blocking.</param>
        /// <param name="runManuallyInMultiNode">The job should run manually in multi node.</param>
        /// <param name="precedingStartupJobAliases">Job aliases which must be completed before this job can run.</param>
        /// <param name="complete">The job is completed.</param>
        public StartupJob(
            string alias,
            bool blocking,
            bool runManuallyInMultiNode = false,
            IEnumerable<string> precedingStartupJobAliases = null,
            bool complete = false)
            : base(default(Guid), default(Instant))
        {
            this.Alias = alias;
            this.Complete = complete;
            this.Blocking = blocking;
            this.RunManuallyInMultiNodeEnvironment = runManuallyInMultiNode;
            this.PrecedingStartupJobAliases = precedingStartupJobAliases;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupJob"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private StartupJob()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the alias of the job.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the startup job has begun executing.
        /// </summary>
        public bool Started { get; set; }

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
        /// Gets or sets the hangfire job ID for this startup job, if one has been created, otherwise null.
        /// </summary>
        public string HangfireJobId { get; set; }

        /// <summary>
        /// Gets or sets the names of the startup jobs which must have been completed before this
        /// startup job can run.
        /// </summary>
        [NotMapped]
        public IEnumerable<string> PrecedingStartupJobAliases
        {
            get
            {
                return string.IsNullOrEmpty(this.PrecedingStartupJobAliasesCommaSeparated)
                    ? Array.Empty<string>()
                    : this.PrecedingStartupJobAliasesCommaSeparated.Split(',');
            }

            set
            {
                this.PrecedingStartupJobAliasesCommaSeparated = value == null
                    ? null
                    : string.Join(",", value);
            }
        }

        /// <summary>
        /// Gets or sets the names of the startup jobs which must have been completed before this
        /// startup job can run, in comma separated format.
        /// </summary>
        [Column("PrecedingStartupJobAliases")]
        public string PrecedingStartupJobAliasesCommaSeparated { get; set; }

        [NotMapped]
        public StartupJobStatus Status
        {
            get
            {
                return this.Complete
                    ? StartupJobStatus.Complete
                    : this.Started
                        ? StartupJobStatus.Started
                        : StartupJobStatus.NotStarted;
            }
        }

        /// <summary>
        /// Completes the job.
        /// </summary>
        public void CompleteJob()
        {
            this.Complete = true;
        }
    }
}
