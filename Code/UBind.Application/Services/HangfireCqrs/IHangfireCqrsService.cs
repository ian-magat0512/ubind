// <copyright file="IHangfireCqrsService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.HangfireCqrs
{
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Provides the service contract to be used by Hangfire  service to schedule a job that runs a command request.
    /// <example>
    /// For example:
    /// <code>
    ///    IBackgroundJobClient.Enqueue{IHangFireCqrsService}(HangfireCqrsService => HangfireCqrsService.Send(request))
    /// </code>
    /// </example>
    /// </summary>
    public interface IHangfireCqrsService
    {
        /// <summary>
        /// Send and execute command requests under the Hangfire job process.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation to obtain the result of the command command .</returns>
        Task Send(ICommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Send and execute command requests under the Hangfire job process with job name attribute.
        /// </summary>
        /// <param name="jobName">The job name for Hangfire job attribute.</param>
        /// <param name="command">The command to be executed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation to obtain the result of the command command .</returns>
        [DisplayName("{0}")]
        Task Send(string jobName, ICommand command, CancellationToken cancellationToken);
    }
}
