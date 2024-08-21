// <copyright file="ReentryUpdaterJobCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Polly;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Represents the handler to execute the re-entry of the updater job.
    /// </summary>
    public class ReentryUpdaterJobCommandHandler : ICommandHandler<ReentryUpdaterJobCommand>
    {
        private readonly IUpdaterJobFactory updaterJobFactory;
        private readonly IStateMachineJobsRepository stateMachineJobRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReentryUpdaterJobCommandHandler"/> class.
        /// </summary>
        /// <param name="updaterJobFactory">updaterJobFactory.</param>
        /// <param name="stateMachineJobRepository">The state machine job repository.</param>
        public ReentryUpdaterJobCommandHandler(IUpdaterJobFactory updaterJobFactory, IStateMachineJobsRepository stateMachineJobRepository)
        {
            this.updaterJobFactory = updaterJobFactory;
            this.stateMachineJobRepository = stateMachineJobRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(ReentryUpdaterJobCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await this.IsStateMachineIdValidAndAvailable(request.JobId))
            {
                return await Task.FromResult(default(Unit));
            }

            var updaterStateMachine = this.updaterJobFactory.GetUpdaterJob(request.UpdaterJobType);
            await updaterStateMachine.ResumeUpdaterJob(request.JobId, cancellationToken);

            return await Task.FromResult(default(Unit));
        }

        private async Task<bool> IsStateMachineIdValidAndAvailable(Guid jobId)
        {
            var policy = Policy
                .Handle<Exception>()
                .OrResult<StateMachineJob>(stateMachineJob => stateMachineJob == null)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(15),
                });

            var validStateMachineJob = await policy.ExecuteAsync(() =>
            {
                var stateMachine = this.stateMachineJobRepository.GetById(jobId);
                return Task.FromResult(stateMachine);
            });

            return validStateMachineJob != null;
        }
    }
}
