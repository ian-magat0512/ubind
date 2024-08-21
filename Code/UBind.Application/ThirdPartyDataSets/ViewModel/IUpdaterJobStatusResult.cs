// <copyright file="IUpdaterJobStatusResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets.ViewModel
{
    using CSharpFunctionalExtensions;
    using UBind.Domain;

    /// <summary>
    /// Provides the contract to be use for the updater job status result view model.
    /// </summary>
    public interface IUpdaterJobStatusResult
    {
        /// <summary>
        /// Gets the job status result.
        /// </summary>
        JobStatusResult JobStatusResult { get; }

        /// <summary>
        /// Gets job Id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets error.
        /// </summary>
        Error Error { get; }

        /// <summary>
        /// Gets the job Status.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Gets a value indicating whether is job is canceled.
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Gets a value indicating whether is the job is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets creation options as per design specs in https://confluence.aptiture.com/x/qgASB.
        /// This for payload consistency with G-NAF implementation, this will be used or refactored in the upcoming tickets.
        /// </summary>
        string CreationOptions { get; }

        /// <summary>
        /// Gets the asyncState as per design specs in https://confluence.aptiture.com/x/qgASB.
        /// This for payload consistency with G-NAF implementation, this will be used or refactored in the upcoming tickets.
        /// </summary>
        string AsyncState { get; }

        /// <summary>
        /// Gets a value indicating whether the jobs faulted. See design specs in https://confluence.aptiture.com/x/qgASB.
        /// This for payload consistency with G-NAF implementation, this will be used or refactored in the upcoming tickets.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// Set the job status result.
        /// </summary>
        /// <param name="jobStatusResult">The job status result.</param>
        void SetJobStatusResult(Result<JobStatusResponse, Error> jobStatusResult);
    }
}
