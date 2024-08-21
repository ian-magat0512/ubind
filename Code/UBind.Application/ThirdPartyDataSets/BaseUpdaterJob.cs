// <copyright file="BaseUpdaterJob.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime;
    using UBind.Application.FiniteStateMachine;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc cref="IUpdaterJob" />
    public abstract class BaseUpdaterJob : UBindStateMachine<string, string>, IUpdaterJob
    {
        private readonly UpdaterJobType updaterJobType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUpdaterJob "/> class.
        /// </summary>
        /// <param name="stateMachineJobRepository">The state machine job repository.</param>
        /// <param name="updaterJobType">The update type.</param>
        protected BaseUpdaterJob(IStateMachineJobsRepository stateMachineJobRepository, UpdaterJobType updaterJobType)
        {
            this.updaterJobType = updaterJobType;
            this.StateMachineJobRepository = stateMachineJobRepository;
        }

        /// <inheritdoc/>
        public IStateMachineJobsRepository StateMachineJobRepository { get; }

        /// <inheritdoc/>
        public string PersistentState { get; set; }

        /// <inheritdoc/>
        public void SetupStateMachineFromReentryState(string reentryState, Action configureStateMachine, Action<string> updateStateToPersistence)
        {
            this.PersistentState = reentryState;
            this.SetupStateMachine(() => this.PersistentState, newState =>
            {
                this.PersistentState = newState;
                updateStateToPersistence(newState);
            });
            configureStateMachine();
        }

        /// <inheritdoc/>
        public JobStatusResponse GetUpdaterJobStatus(Guid id)
        {
            var stateMachineJob = this.StateMachineJobRepository.GetByIdAndJobType(id, this.updaterJobType.Humanize());

            if (stateMachineJob == null)
            {
                throw new ErrorException(Errors.ThirdPartyDataSets.NotFound(id));
            }

            var createUpdateJobResponseVm = new JobStatusResponse(stateMachineJob);
            return createUpdateJobResponseVm;
        }

        /// <inheritdoc/>
        public UpdaterJobStatusResult GetUpdaterJobStatusResult(Guid id)
        {
            var jobStatusResponseVm = this.StateMachineJobRepository.GetById(id);

            return new UpdaterJobStatusResult(jobStatusResponseVm);
        }

        /// <inheritdoc/>
        public IEnumerable<JobStatusResponse> GetListUpdaterJobsStatuses()
        {
            var stateMachineJobs = this.StateMachineJobRepository.GetListByJobType(this.updaterJobType.Humanize());

            foreach (var stateMachineJob in stateMachineJobs)
            {
                yield return new JobStatusResponse(stateMachineJob);
            }
        }

        /// <inheritdoc/>
        public StateMachineJob CreateAndSaveUpdaterJobStateMachine(
            Guid id,
            Instant createdTimestamp,
            string hangFireId,
            UpdaterJobType updaterJobType,
            string initialState,
            string datasetUrl,
            string stateMachineJobManifest)
        {
            var stateMachineJob = new StateMachineJob(id, createdTimestamp, updaterJobType.Humanize(), hangFireId, datasetUrl, stateMachineJobManifest);
            stateMachineJob.SetState(initialState);

            this.StateMachineJobRepository.Add(stateMachineJob);
            this.StateMachineJobRepository.SaveChanges();
            return stateMachineJob;
        }

        /// <inheritdoc/>
        public async Task WaitForCompletion(CancellationToken cancellationToken, Func<bool> isComplete)
        {
            while (true)
            {
                await Task.Delay(1000, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    if (isComplete())
                    {
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public abstract StateMachineJob CreateAndSaveUpdaterJobStateMachine(
            Guid id,
            Instant createdTimestamp,
            string hangFireId,
            IUpdaterJobManifest updaterJobManifest);

        /// <inheritdoc/>
        public abstract Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract Task<JobStatusResponse> CancelUpdaterJob(Guid updaterJobId);
    }
}
