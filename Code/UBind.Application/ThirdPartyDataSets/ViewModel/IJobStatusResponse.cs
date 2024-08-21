// <copyright file="IJobStatusResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.ViewModel
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Provides the contract to be use for the job status response view model.
    /// </summary>
    public interface IJobStatusResponse
    {
        /// <summary>
        /// Gets the Hangfire job Id.
        /// </summary>
        string HangfireJobId { get; }

        /// <summary>
        /// Gets the version step.
        /// </summary>
        string Step { get; }

        /// <summary>
        /// Gets the start time of the version update.
        /// </summary>
        string StartDateTime { get; }

        /// <summary>
        /// Gets start time as the number of ticks since the epoch..
        /// </summary>
        long? StartTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the end time of version update.
        /// </summary>
        string EndDateTime { get; }

        /// <summary>
        /// Gets end time as the number of ticks since the epoch.
        /// </summary>
        long? EndTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the Serialized error object of version update.
        /// </summary>
        string SerializedError { get; }

        /// <summary>
        /// Gets a value indicating whether the data set is downloaded or not.
        /// </summary>
        bool IsDownloaded { get; }

        /// <summary>
        /// Gets a value indicating whether the downloaded data set is extracted or not.
        /// </summary>
        bool IsExtracted { get; }

        /// <summary>
        /// Gets or sets a value the primary data set downloaded url.
        /// </summary>
        string DatasetUrl { get; set; }

        /// <summary>
        /// Gets the Deserialized error object of version update.
        /// </summary>
        Error Error { get; }

        /// <summary>
        /// Gets job Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the created time.
        /// </summary>
        string CreatedDateTime { get; }

        /// <summary>
        /// Gets end time as the number of ticks since the epoch.
        /// </summary>
        long? CreatedTicksSinceEpoch { get; }
    }
}
