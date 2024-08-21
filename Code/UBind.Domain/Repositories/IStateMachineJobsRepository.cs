// <copyright file="IStateMachineJobsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// Provides the contract to be use for the state machine jobs repository handling the Entity related operations.
    /// </summary>
    public interface IStateMachineJobsRepository
    {
        /// <summary>
        /// Get the state machine job entity by id asynchronously.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation  to obtain the entity.</returns>
        Task<StateMachineJob> GetByIdAsync(Guid id);

        /// <summary>
        /// Get the state machine job entity by id synchronously.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <returns>A <see cref="Task"/> representing the synchronous operation  to obtain the entity.</returns>
        StateMachineJob GetById(Guid id);

        /// <summary>
        /// Get the state machine job entity by id and job type.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="jobType">The state machine job type.</param>
        /// <returns>A <see cref="Task"/> representing the synchronous operation  to obtain the entity.</returns>
        StateMachineJob? GetByIdAndJobType(Guid id, string jobType);

        /// <summary>
        /// Save changes to state machine job repository synchronously.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Save changes to state machine job repository asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation  to save changes in the repository.</returns>
        Task SaveChangesAsync();

        /// <summary>
        /// Add state machine job entity  in the repository.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Add(StateMachineJob entity);

        /// <summary>
        /// Get the list of state machine jobs asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of state machine jobs.</returns>
        Task<IEnumerable<StateMachineJob>> GetListAsync();

        /// <summary>
        /// Get the list of state machine jobs synchronously.
        /// </summary>
        /// <returns>Return the result of the request to obtain the list of state machine jobs.</returns>
        IEnumerable<StateMachineJob> GetList();

        /// <summary>
        /// Get the list of state machine jobs asynchronously.
        /// </summary>
        /// <param name="jobType">The state machine job type.</param>
        /// <returns>A <see cref="Task"/> representing the result of the request to obtain the list of state machine jobs.</returns>
        Task<IEnumerable<StateMachineJob>> GetListByJobTypeAsync(string jobType);

        /// <summary>
        /// Get the list of state machine jobs synchronously.
        /// </summary>
        /// <param name="jobType">The state machine job type.</param>
        /// <returns>Return the result of the request to obtain the list of state machine jobs.</returns>
        IEnumerable<StateMachineJob> GetListByJobType(string jobType);

        /// <summary>
        /// Update state machine current state.
        /// </summary>
        /// <param name="stateMachineJobId">The id of the state machine job.</param>
        /// <param name="state">The new state to be persisted.</param>
        void UpdateStateMachineCurrentState(Guid stateMachineJobId, string state);
    }
}
