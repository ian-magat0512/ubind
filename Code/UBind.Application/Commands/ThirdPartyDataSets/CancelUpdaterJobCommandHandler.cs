// <copyright file="CancelUpdaterJobCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the handler to cancel the updater job.
    /// </summary>
    public class CancelUpdaterJobCommandHandler : ICommandHandler<CancelUpdaterJobCommand, JobStatusResponse>
    {
        private readonly IUpdaterJobFactory updaterJobFactory;

        private readonly IBackgroundJobClient backgroundJobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelUpdaterJobCommandHandler"/> class.
        /// </summary>
        /// <param name="updaterJobFactory">The updater job factory.</param>
        /// <param name="backgroundJobClient">The Hangfire background job client.</param>
        public CancelUpdaterJobCommandHandler(IUpdaterJobFactory updaterJobFactory, IBackgroundJobClient backgroundJobClient)
        {
            this.updaterJobFactory = updaterJobFactory;
            this.backgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc/>
        public async Task<JobStatusResponse> Handle(CancelUpdaterJobCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updaterStateMachine = this.updaterJobFactory.GetUpdaterJob(request.UpdaterJobType);
            await updaterStateMachine.CancelUpdaterJob(request.UpdateJobId);

            var jobStatus = updaterStateMachine.GetUpdaterJobStatus(request.UpdateJobId);
            this.backgroundJobClient.Delete(jobStatus.HangfireJobId);

            return await Task.FromResult(jobStatus);
        }
    }
}
