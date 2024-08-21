// <copyright file="IHangfireCqrsJobService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.HangfireCqrs
{
    using System.Threading;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Provides the service contract to be used by Hangfire CQRS job service to create a background job based on a specified command request.
    /// </summary>
    public interface IHangfireCqrsJobService
    {
        /// <summary>
        /// Creates a background job based on a specified command request.
        /// </summary>
        /// <param name="command">The command request to be executed in the background job.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The unique identifier of the created job.</returns>
        string EnqueueRequest(ICommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a background job based on a specified command request and a job name.
        /// </summary>
        /// <param name="jobName">The job name for the background job to be created.</param>
        /// <param name="command">The command request to be executed in the background job.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The unique identifier of the created job.</returns>
        string EnqueueRequest(string jobName, ICommand command, CancellationToken cancellationToken);
    }
}
