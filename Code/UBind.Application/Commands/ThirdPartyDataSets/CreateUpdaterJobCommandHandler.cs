// <copyright file="CreateUpdaterJobCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Services.HangfireCqrs;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the handler processing for creation of the updater job.
    /// </summary>
    public class CreateUpdaterJobCommandHandler : ICommandHandler<CreateUpdaterJobCommand, UpdaterJobStatusResult>
    {
        private readonly IHangfireCqrsJobService hangFireCqrsJobService;
        private readonly IClock clock;

        private readonly IUpdaterJobFactory updaterJobFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUpdaterJobCommandHandler"/> class.
        /// </summary>
        /// <param name="hangfireCqrsJobService">The Hangfire CQRS job service.</param>
        /// <param name="clock">A clock to get the current time instant.</param>
        /// <param name="updaterJobFactory">The updater job factory.</param>
        public CreateUpdaterJobCommandHandler(IHangfireCqrsJobService hangfireCqrsJobService, IClock clock, IUpdaterJobFactory updaterJobFactory)
        {
            this.hangFireCqrsJobService = hangfireCqrsJobService;
            this.clock = clock;
            this.updaterJobFactory = updaterJobFactory;
        }

        /// <inheritdoc/>
        public async Task<UpdaterJobStatusResult> Handle(CreateUpdaterJobCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updaterStateMachine = this.updaterJobFactory.GetUpdaterJob(request.UpdaterJobType);

            var stateMachineJobId = Guid.NewGuid();
            var hangFireId = this.hangFireCqrsJobService.EnqueueRequest(
                $"Resume Data set Updater Job {updaterStateMachine.GetType()}",
                new ReentryUpdaterJobCommand(request.UpdaterJobType, stateMachineJobId),
                cancellationToken);

            var stateMachineJob = updaterStateMachine.CreateAndSaveUpdaterJobStateMachine(
                stateMachineJobId,
                this.clock.GetCurrentInstant(),
                hangFireId,
                request.UpdaterJobManifest);
            var updaterJobStatus = updaterStateMachine.GetUpdaterJobStatusResult(stateMachineJob.Id);
            return await Task.FromResult(updaterJobStatus);
        }
    }
}
