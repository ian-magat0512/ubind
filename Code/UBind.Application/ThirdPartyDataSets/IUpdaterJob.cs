// <copyright file="IUpdaterJob.cs" company="uBind">
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
    using NodaTime;
    using Stateless;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Provides the contract to be use for the services handling  third party data sets updater  like  RedBook , GNAF, Glass's Guide and of similar use case.
    /// </summary>
    public interface IUpdaterJob
    {
        /// <summary>
        /// Gets or sets get the current state of the state machine.
        /// </summary>
        /// <returns>Returns the current state machine state.</returns>
        string PersistentState { get; set; }

        /// <summary>
        /// Gets the state machine job repository.
        /// </summary>
        IStateMachineJobsRepository StateMachineJobRepository { get; }

        /// <summary>
        /// Get the job updater status by job id.
        /// </summary>
        /// <param name="id">The job id.</param>
        /// <returns>UpdaterJobResponseVm.</returns>
        /// <returns>Return updater job status.</returns>
        JobStatusResponse GetUpdaterJobStatus(Guid id);

        /// <summary>
        /// Get the job updater status by job id with result data.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>Return updater job status with result data.</returns>
        UpdaterJobStatusResult GetUpdaterJobStatusResult(Guid id);

        /// <summary>
        /// Get the list of updater job status.
        /// </summary>
        /// <returns>Return the result of the request to obtain the list of updater job status.</returns>
        IEnumerable<JobStatusResponse> GetListUpdaterJobsStatuses();

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        void SetupStateMachine(Func<string> stateAccessor, Action<string> stateMutator);

        /// <summary>
        /// Get the state configuration of a given sate.
        /// </summary>
        /// <param name="configureState">The state machine state.</param>
        /// <returns>Return the State Configuration of the given state machine state.</returns>
        StateMachine<string, string>.StateConfiguration Configure(string configureState);

        /// <summary>
        /// Invoke or fire a trigger from a given trigger value.
        /// </summary>
        /// <param name="trigger">The trigger value to be invoke in the state machine.</param>
        void Fire(string trigger);

        /// <summary>
        /// Create and Save updater job.
        /// </summary>
        /// <param name="id">The updater job id.</param>
        /// <param name="createdTimestamp">The created time of updater job.</param>
        /// <param name="hangFireId">The Hangfire id.</param>
        /// <param name="updaterJobType">The updater job type.</param>
        /// <param name="initialState">The initial state of the job.</param>
        /// <param name="datasetUrl">The dataset URL.</param>
        /// <param name="stateMachineJobManifest">The state machine job manifest in string or json string format.</param>
        /// <returns>Returns the state machine job created.</returns>
        StateMachineJob CreateAndSaveUpdaterJobStateMachine(
            Guid id,
            Instant createdTimestamp,
            string hangFireId,
            UpdaterJobType updaterJobType,
            string initialState,
            string datasetUrl,
            string stateMachineJobManifest);

        /// <summary>
        /// Setup state machine from re-entry state.
        /// </summary>
        /// <param name="reentryState">The reentry state.</param>
        /// <param name="configureStateMachine">The action for configuring state.</param>
        /// <param name="updateStateToPersistence">The action for updating state to persistence.</param>
        void SetupStateMachineFromReentryState(string reentryState, Action configureStateMachine, Action<string> updateStateToPersistence);

        /// <summary>
        /// WaitForCompletion.
        /// </summary>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <param name="isComplete">isComplete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task WaitForCompletion(CancellationToken cancellationToken, Func<bool> isComplete);

        /// <summary>
        /// Create and Save updater job.
        /// </summary>
        /// <param name="id">The updater job id.</param>
        /// <param name="createdTimestamp">The created time of updater job.</param>
        /// <param name="hangFireId">The Hangfire id.</param>
        /// <param name="updaterJobManifest">The updater manifest.</param>
        /// <returns>Returns the state machine job created.</returns>
        StateMachineJob CreateAndSaveUpdaterJobStateMachine(Guid id, Instant createdTimestamp, string hangFireId, IUpdaterJobManifest updaterJobManifest);

        /// <summary>
        /// Resume  re-entrant updater job by job id.
        /// </summary>
        /// <param name="updaterJobId">The updater job id to be cancel.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ResumeUpdaterJob(Guid updaterJobId, CancellationToken cancellationToken);

        /// <summary>
        /// Cancel an updater job by job id.
        /// </summary>
        /// <param name="updaterJobId">The updater job id to be cancel.</param>
        /// <returns>Return the job status of the cancelled job.</returns>
        Task<JobStatusResponse> CancelUpdaterJob(Guid updaterJobId);
    }
}
