// <copyright file="IStartupJobRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// A repository for startup jobs.
    /// </summary>
    public interface IStartupJobRepository
    {
        StartupJob GetStartupJobByAlias(string alias);

        /// <summary>
        /// gets an incomplete startup job by alias.
        /// </summary>
        /// <param name="alias">The alias of the startup job.</param>
        /// <returns>A startup job.</returns>
        StartupJob GetIncompleteByAlias(string alias);

        /// <summary>
        /// gets all incomplete startup jobs.
        /// </summary>
        /// <returns>All non-complete startup jobs.</returns>
        IEnumerable<StartupJob> GetIncompleteStartupJobs();

        /// <summary>
        /// Gets all startup jobs that are dependent on the given startup job.
        /// They would be considered dependent if the given startup job is in their preceding startup job aliases.
        /// </summary>
        /// <param name="startupJobAlias"></param>
        /// <returns></returns>
        IEnumerable<StartupJob> GetJobsDependentOn(string startupJobAlias);

        /// <summary>
        /// Records that the hangfire job with the given alias has started executing.
        /// </summary>
        /// <param name="alias">The alias of the startup job.</param>
        void StartJobByAlias(string alias);

        /// <summary>
        /// Completes a job by alias.
        /// </summary>
        /// <param name="alias">The alias of the startup job.</param>
        void CompleteJobByAlias(string alias);

        /// <summary>
        /// Records the hangfire job ID of the startup job with the given alias.
        /// </summary>
        /// <param name="startupJobAlias">The startup job alias.</param>
        /// <param name="hangfireJobId">The hangfire job ID.</param>
        void RecordHangfireJobId(string startupJobAlias, string hangfireJobId);
    }
}
