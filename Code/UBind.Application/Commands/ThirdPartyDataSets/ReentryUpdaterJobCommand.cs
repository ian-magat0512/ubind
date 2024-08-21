// <copyright file="ReentryUpdaterJobCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to execute the re-entry of the updater job.
    /// </summary>
    public class ReentryUpdaterJobCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReentryUpdaterJobCommand"/> class.
        /// </summary>
        /// <param name="updaterJobType">The updater job type.</param>
        /// <param name="jobId">The job id.</param>
        public ReentryUpdaterJobCommand(Type updaterJobType, Guid jobId)
        {
            this.UpdaterJobType = updaterJobType;
            this.JobId = jobId;
        }

        /// <summary>
        /// Gets the job Id.
        /// </summary>
        public Guid JobId { get; }

        /// <summary>
        /// Gets the updater job type.
        /// </summary>
        public Type UpdaterJobType { get; }
    }
}
