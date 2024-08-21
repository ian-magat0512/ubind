// <copyright file="IStartupJobRunner.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.StartupJobs
{
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Intializes Startup jobs.
    /// </summary>
    public interface IStartupJobRunner
    {
        /// <summary>
        /// Run existing startup jobs.
        /// </summary>
        Task RunJobs();

        Task RunJobByAlias(string startupJobAlias, bool force = false);

        /// <summary>
        /// Enqueue the hangfire job that will validate and run startup jobs.
        /// </summary>
        /// <param name="startupJobAlias">The startup job alias.</param>
        /// <param name="force">Indicates whether to still run this job, even it's already started.</param>
        void EnqueueStartupJob(string startupJobAlias, bool force = false);

        /// <summary>
        /// Executes a startup job and marks it completed.
        /// This is not to be called directly, only by hangfire.
        /// </summary>
        /// <param name="startupJobAlias">The startup job alias.</param>
        /// <param name="cancellationToken">The cancellationToken that gets replaced by hangfire and cancelled when the server is shut down or job is deleted.</param>
        Task ExecuteStartupJobAndMarkCompleted(string startupJobAlias, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an email if there are incomplete startup jobs.
        /// </summary>
        void NotifyIncompleteJobs();

        void ThrowIfPrecedingStartupJobsHaveNotBeenCompleted(StartupJob startupJob);
    }
}
